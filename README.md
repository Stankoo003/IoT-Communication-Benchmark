# IoT Communication Benchmark

**Komparativna analiza sinhronih komunikacionih paradigmi u IoT mikroservisnim sistemima**

Projekat poredi performanse tri protokola — **REST**, **gRPC** i **GraphQL** — na identičnom IoT workload-u. Sva tri servisa dele istu PostgreSQL bazu i izlažu iste tri operacije (ingestion, selective monitoring, heavy querying), što omogućava direktno poređenje latencije, mrežnog saobraćaja i CPU/RAM zauzimanja.

---

## Arhitektura

```
k6 load tests
      │
      ├──► REST (ASP.NET Core C#)    :5001  ─┐
      ├──► gRPC (ASP.NET Core C#)    :5002  ─┼──► PostgreSQL 16  :5432
      └──► GraphQL (Node.js/Apollo)  :5003  ─┘
```

Sve četiri komponente se pokreću kroz Docker Compose. Servisi čekaju na PostgreSQL healthcheck pre starta.

---

## Tech Stack

| Komponenta | Tehnologija | Port |
|---|---|---|
| REST servis | ASP.NET Core (.NET 10) + Dapper + Swagger | 5001 |
| gRPC servis | ASP.NET Core (.NET 10) + Protobuf | 5002 |
| GraphQL servis | Node.js 22 + Apollo Server 4 | 5003 |
| Baza podataka | PostgreSQL 16 | 5432 |

---

## Dataset

**Simulated IoT Environmental Sensor Dataset** — 500 000 očitavanja sa 50 uređaja kroz vremenski period.

| Kolona | Tip | Opis |
|---|---|---|
| `timestamp` | TIMESTAMPTZ | Vreme očitavanja |
| `device_id` | VARCHAR(50) | Identifikator uređaja |
| `temperature` | NUMERIC | Temperatura u °C |
| `humidity` | NUMERIC | Vlažnost u % |
| `pressure` | NUMERIC | Pritisak u hPa |
| `light` | INTEGER | Osvetljenost u lux |
| `sound` | INTEGER | Buka u dB |
| `motion` | SMALLINT | Detekcija pokreta (0/1) |
| `battery` | NUMERIC | Nivo baterije u % |
| `location` | VARCHAR | Lokacija uređaja |

---

## Baza podataka

Shema je optimizovana za IoT upitne obrasce:

```sql
-- Indeksi
idx_sensor_timestamp    -- za range upite po vremenu (Scenario C)
idx_sensor_device_id    -- za filtriranje po uređaju (Scenario B)
idx_sensor_device_time  -- kompozitni, najčešći IoT obrazac

-- Views
latest_readings    -- poslednje očitavanje po uređaju (DISTINCT ON)
hourly_aggregates  -- satne agregacije (AVG, MIN, SUM, COUNT)
```

---

## Scenariji

### Scenario A — High-Frequency Ingestion
Simulacija IoT uređaja koji šalje podatke u kratkim intervalima. Meri se brzina upisa i overhead serijalizacije po protokolu.

- REST: `POST /api/readings`
- gRPC: `SensorService.IngestReading`
- GraphQL: `mutation { ingestReading(...) }`

### Scenario B — Selective Monitoring
Klijent sa ograničenom vezom traži samo određena polja (npr. samo `temperature` i `humidity` od 10 dostupnih). GraphQL ovde ima prednost — klijent eksplicitno definiše koje kolone želi, bez over-fetching-a.

- REST: `GET /api/readings?deviceId=X&limit=N`
- gRPC: `SensorService.GetReadings`
- GraphQL: `query { readings(deviceId: "X") { temperature humidity } }`

### Scenario C — Heavy Querying
Složeni agregacijski upiti nad velikim opsegom istorijskih podataka. Servisi koriste `hourly_aggregates` view direktno.

- REST: `GET /api/aggregates?deviceId=X&from=...&to=...`
- gRPC: `SensorService.GetAggregates`
- GraphQL: `query { aggregates(deviceId: "X") { hour avgTemperature } }`

---

## API Referenca

### REST (OpenAPI/Swagger)

Swagger UI dostupan na `http://localhost:5001/swagger`.

| Method | Endpoint | Scenario | Opis |
|---|---|---|---|
| POST | `/api/readings` | A | Upiši novo očitavanje |
| GET | `/api/readings` | B | Lista očitavanja (filter: deviceId, from, to, limit) |
| GET | `/api/readings/{deviceId}/latest` | B | Poslednje očitavanje uređaja |
| GET | `/api/devices` | — | Lista svih uređaja |
| GET | `/api/aggregates` | C | Satne agregacije (filter: deviceId, from, to) |

### gRPC (Protobuf)

Proto fajl: [`grpc-service/Protos/iot_sensor.proto`](grpc-service/Protos/iot_sensor.proto)

```protobuf
service SensorService {
  rpc IngestReading    (IngestRequest)      returns (IngestResponse);
  rpc GetReadings      (GetReadingsRequest) returns (GetReadingsResponse);
  rpc GetLatestReading (LatestRequest)      returns (SensorReading);
  rpc GetAggregates    (AggregateRequest)   returns (AggregateResponse);
}
```

Testiranje sa `grpcurl`:
```bash
grpcurl -plaintext -d '{"limit": 3}' localhost:5002 iot.SensorService/GetReadings
```

### GraphQL

Apollo Sandbox dostupan na `http://localhost:5003/`.

```graphql
# Scenario B – klijent bira samo polja koja mu trebaju
query {
  readings(deviceId: "Device_1", limit: 10) {
    timestamp
    temperature
    humidity
  }
}

# Scenario C – agregacije
query {
  aggregates(deviceId: "Device_1") {
    hour
    avgTemperature
    avgHumidity
    readingCount
  }
}

# Scenario A – upis
mutation {
  ingestReading(deviceId: "Device_1", temperature: 22.5, humidity: 60.0, motion: 0) {
    id
  }
}
```

---

## Pokretanje

### Zahtevi
- Docker + Docker Compose

### Start

```bash
git clone https://github.com/Stankoo003/IoT-Communication-Benchmark.git
cd IoT-Communication-Benchmark

docker compose up --build
```

PostgreSQL automatski učitava semu i seed podatke iz `/database/` foldera pri prvom startu.

### Provera

```bash
docker compose ps

# REST
curl http://localhost:5001/api/devices

# GraphQL
curl -X POST http://localhost:5003/ \
  -H "Content-Type: application/json" \
  -d '{"query":"{ devices { deviceId location } }"}'

# gRPC
grpcurl -plaintext localhost:5002 list
```

---

## Struktura projekta

```
/
├── docker-compose.yml
├── database/
│   ├── 01_schema.sql          # Shema + indeksi + views
│   ├── 02_seed_data.sql       # Seed podaci (devices tabela)
│   └── real_time_data.csv     # Dataset (500k redova)
├── rest-service/              # ASP.NET Core REST API
│   ├── Program.cs             # Minimal API endpointi
│   ├── Models/                # SensorReading, HourlyAggregate, SensorReadingInput
│   └── Dockerfile
├── grpc-service/              # ASP.NET Core gRPC
│   ├── Program.cs
│   ├── Protos/iot_sensor.proto
│   ├── Services/SensorService.cs
│   └── Dockerfile
├── graphql-service/           # Node.js + Apollo Server
│   ├── index.js
│   ├── schema.js              # GraphQL type definitions
│   ├── resolvers.js           # Query/Mutation implementacija
│   └── Dockerfile
└── k6-tests/                  # Load test skripte
```

---

## Evaluacija performansi

### k6 load testovi

Folder [`k6-tests/`](k6-tests/) sadrži **9 skripti** (3 protokola × 3 scenarija). Sve dele [`lib/config.js`](k6-tests/lib/config.js) — base URL-ovi i randomizovani workload generator.

```bash
# Pojedinacni test
k6 run k6-tests/scenario-a-rest.js

# Sa parametrima — VUs i trajanje
VUS=100 DURATION=1m k6 run k6-tests/scenario-b-graphql.js

# Sve kombinacije (3 protokola × 3 scenarija × 3 nivoa VUs = 27 testova)
./k6-tests/run-all.sh
```

`run-all.sh` snima JSON summary svakog testa u `k6-tests/results/<scenario>_vus<N>.json`. Nivoi: **10, 100, 500 VUs**, default trajanje **30s** (override sa `DURATION=1m`).

**Ključne metrike** koje k6 prijavljuje:
- `http_req_duration` / `grpc_req_duration` — avg, p(95), max latencija
- `http_reqs` — ukupan broj zahteva → **RPS** = `count / duration`
- `data_sent` / `data_received` — wire-size payload-a (Protobuf vs JSON)
- `checks` — pass rate validacija odgovora

Thresholds postavljeni u `lib/config.js` (p95 < 2s, fail rate < 1%, checks > 99%).

### Veličina payload-a

```bash
./k6-tests/payload-size.sh
```

Poredi **isti workload** (10 očitavanja Device_1) preko REST / GraphQL (selektivno + sva polja) / gRPC. Za precizan Protobuf wire-size koristi `data_received` iz k6 izveštaja ili Wireshark filter `tcp.port == 5002`.

### CPU/RAM monitoring

```bash
# Pokreni paralelno sa k6 testom
./k6-tests/monitor-stats.sh > k6-tests/results/stats.csv &
k6 run k6-tests/scenario-a-grpc.js
kill %1
```

CSV output: `timestamp, container, cpu_perc, mem_usage, mem_perc, net_io` po sekundi za sva 4 kontejnera (REST, gRPC, GraphQL, Postgres).

### Zahtevi za testiranje

- [`k6`](https://k6.io/docs/getting-started/installation/) ≥ v0.49 (Protobuf well-known types autoload)
- `grpcurl` (opciono, za payload-size script)
