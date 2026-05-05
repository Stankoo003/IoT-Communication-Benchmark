#!/usr/bin/env bash
# Loguje `docker stats` po sekundi u CSV. Pokreni paralelno sa k6:
#   ./monitor-stats.sh > results/stats.csv &
#   k6 run scenario-a-rest.js
#   kill %1
set -euo pipefail

CONTAINERS=(iot_rest iot_grpc iot_graphql iot_postgres)
INTERVAL="${INTERVAL:-1}"

echo "timestamp,container,cpu_perc,mem_usage,mem_perc,net_io"
while true; do
  ts=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
  docker stats --no-stream \
    --format "{{.Container}};{{.CPUPerc}};{{.MemUsage}};{{.MemPerc}};{{.NetIO}}" \
    "${CONTAINERS[@]}" \
    | while IFS=';' read -r c cpu mem memp net; do
      echo "$ts,$c,$cpu,\"$mem\",$memp,\"$net\""
    done
  sleep "$INTERVAL"
done
