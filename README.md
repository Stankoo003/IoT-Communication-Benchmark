# IoT Communication Benchmark

## Komparativna analiza sinhronih komunikacionih paradigmi u IoT mikroservisnim sistemima

Projekat za predmet **Internet stvari i servisa** — poređenje REST, gRPC i GraphQL protokola u IoT kontekstu.

---

## Tech Stack

| Servis | Tehnologija | Port |
|---|---|---|
| REST | ASP.NET Core (C#) | 5001 |
| gRPC | ASP.NET Core (C#) | 5002 |
| GraphQL | Node.js + Apollo Server | 5003 |
| Baza podataka | PostgreSQL 16 | 5432 |

---

## Dataset

**Simulated IoT Environmental Sensor Dataset** — 50 uređaja, 10 senzorskih vrednosti, vremenski serijalizovan.

Kolone: `timestamp, device_id, temperature, humidity, pressure, light, sound, motion, battery, location`

> ⚠️ Dataset (`real_time_data.csv`) nije u repozitorijumu. Preuzeti sa Kaggle-a i postaviti u `/database/` folder.

---

## Pokretanje

```bash
# 1. Kloniraj repo
git clone https://github.com/Stankoo003/IoT-Communication-Benchmark.git
cd IoT-Communication-Benchmark

# 2. Postavi dataset
cp /path/to/real_time_data.csv ./database/

# 3. Pokreni sve servise
docker-compose up --build
```

## Servisi

- REST API + Swagger: http://localhost:5001/swagger
- gRPC: http://localhost:5002
- GraphQL Playground: http://localhost:5003/graphql
- PostgreSQL: localhost:5432

---

## Struktura projekta

```
/
├── docker-compose.yml
├── README.md
├── database/
│   ├── 01_schema.sql          # PostgreSQL shema + indeksi
│   ├── 02_seed_data.sql       # Seed podaci
│   └── real_time_data.csv     # Dataset (nije u repo)
├── rest-service/              # ASP.NET Core REST API
├── grpc-service/              # ASP.NET Core gRPC
├── graphql-service/           # Node.js + Apollo GraphQL
└── k6-tests/                  # Load test skripte
```

---

## IoT Scenariji

- **Scenario A** — High-Frequency Ingestion: Brzi upis podataka sa uređaja
- **Scenario B** — Selective Monitoring: Klijent traži samo 2 od 10 senzorskih polja
- **Scenario C** — Heavy Querying: Agregacijski upiti nad istorijskim podacima
