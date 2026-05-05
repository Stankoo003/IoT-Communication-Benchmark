#!/usr/bin/env bash
# Pokreni sve k6 testove: 3 protokola x 3 scenarija x 3 nivoa VUs.
# JSON summary svakog testa ide u results/.
set -euo pipefail

cd "$(dirname "$0")"
mkdir -p results

LEVELS=("10" "100" "500")
DURATION="${DURATION:-30s}"

SCENARIOS=(
  scenario-a-rest scenario-a-grpc scenario-a-graphql
  scenario-b-rest scenario-b-grpc scenario-b-graphql
  scenario-c-rest scenario-c-grpc scenario-c-graphql
)

for s in "${SCENARIOS[@]}"; do
  for v in "${LEVELS[@]}"; do
    out="results/${s}_vus${v}.json"
    echo "=== $s @ ${v} VUs (${DURATION}) -> $out"
    VUS="$v" DURATION="$DURATION" k6 run --summary-export="$out" "$s.js" || true
  done
done

echo
echo "Gotovo. Rezultati: $(pwd)/results"
