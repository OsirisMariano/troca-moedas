#!/usr/bin/env bash
set -euo pipefail

# Mede a cobertura das camadas de negócio (Domain, Application e Infrastructure).
# A camada Presentation fica fora do cálculo por ser majoritariamente saída de terminal.
#
# Uso:
#   ./scripts/coverage.sh
#   COVERAGE_THRESHOLD=85 ./scripts/coverage.sh

cd "$(dirname "$0")/.."

mkdir -p coverage

DOCKER_IMAGE="${DOTNET_SDK_IMAGE:-mcr.microsoft.com/dotnet/sdk:10.0}"
THRESHOLD="${COVERAGE_THRESHOLD:-80}"

docker run --rm \
  -v "$PWD":/app \
  -w /app \
  -e DOTNET_CLI_TELEMETRY_OPTOUT=1 \
  -e DOTNET_NOLOGO=1 \
  "$DOCKER_IMAGE" \
  dotnet test --nologo \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput=/app/coverage/coverage \
    /p:Threshold="$THRESHOLD" \
    /p:ThresholdType=line \
    /p:ThresholdStat=total \
    /p:Exclude="[TrocaMoedas.Presentation]*"

echo
echo "Relatório: coverage/coverage.cobertura.xml"
