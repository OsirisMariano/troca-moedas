# syntax=docker/dockerfile:1

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaura apenas com os arquivos de projeto para aproveitar o cache de camadas.
COPY troca-moedas.sln ./
COPY src/TrocaMoedas.Presentation/TrocaMoedas.Presentation.csproj src/TrocaMoedas.Presentation/
COPY src/TrocaMoedas.Application/TrocaMoedas.Application.csproj src/TrocaMoedas.Application/
COPY src/TrocaMoedas.Domain/TrocaMoedas.Domain.csproj src/TrocaMoedas.Domain/
COPY src/TrocaMoedas.Infrastructure/TrocaMoedas.Infrastructure.csproj src/TrocaMoedas.Infrastructure/
RUN dotnet restore src/TrocaMoedas.Presentation/TrocaMoedas.Presentation.csproj

COPY src/ src/

RUN dotnet publish src/TrocaMoedas.Presentation/TrocaMoedas.Presentation.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

# Usuário não-root já presente nas imagens oficiais .NET 8+.
RUN mkdir -p /app/data && chown -R "$APP_UID" /app

COPY --from=build --chown=$APP_UID /app/publish ./

USER $APP_UID

ENV TERM=xterm-256color \
    DOTNET_NOLOGO=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1

VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "TrocaMoedas.Presentation.dll"]
