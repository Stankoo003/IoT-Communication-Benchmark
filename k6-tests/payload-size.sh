#!/usr/bin/env bash
# Uporedi velicinu *aplikativnog* odgovora (telo, bez TLS/HTTP overhead-a)
# za isti workload (Scenario B, 10 ocitavanja Device_1).
#
# Za REST/GraphQL: curl ispisuje cisti JSON, wc -c je tacna velicina.
# Za gRPC: grpcurl renderuje JSON, pa NIJE pravi Protobuf payload.
# Stvarnu protobuf velicinu ocitati iz k6 metrike `data_received` u
# results/scenario-b-grpc_vus10.json (polje data_received / iterations)
# ili Wireshark-om: filter "tcp.port == 5002".
set -euo pipefail

REST_URL="${REST_URL:-http://localhost:5001}"
GRAPHQL_URL="${GRAPHQL_URL:-http://localhost:5003}"
GRPC_HOST="${GRPC_HOST:-localhost:5002}"

bytes() { wc -c | tr -d ' '; }

echo "=== REST (svih 11 polja, JSON) ==="
rest=$(curl -s "$REST_URL/api/readings?deviceId=Device_1&limit=10" | bytes)
echo "$rest B"

echo "=== GraphQL - selektivno (3 polja, JSON) ==="
gql_sel=$(curl -s -X POST "$GRAPHQL_URL/" \
  -H "Content-Type: application/json" \
  -d '{"query":"{ readings(deviceId:\"Device_1\", limit:10) { timestamp temperature humidity } }"}' \
  | bytes)
echo "$gql_sel B"

echo "=== GraphQL - sva polja (JSON) ==="
gql_all=$(curl -s -X POST "$GRAPHQL_URL/" \
  -H "Content-Type: application/json" \
  -d '{"query":"{ readings(deviceId:\"Device_1\", limit:10) { id timestamp deviceId temperature humidity pressure light sound motion battery location } }"}' \
  | bytes)
echo "$gql_all B"

echo "=== gRPC (Protobuf - aproksimacija preko grpcurl JSON) ==="
if command -v grpcurl >/dev/null; then
  grpc_json=$(grpcurl -plaintext -d '{"device_id":"Device_1","limit":10}' \
    "$GRPC_HOST" iot.SensorService/GetReadings | bytes)
  echo "$grpc_json B (JSON render, ne pravi Protobuf)"
else
  echo "grpcurl nije instaliran"
fi

echo
echo "Napomena: za precizan Protobuf wire-size pogledaj data_received u k6 JSON izvestaju."
