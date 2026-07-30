# Autor: Anderson Pereira Silva
# Data: 29/07/2026
# Descrição: Imagem de produção ChargeDesk Platform (Host.Api .NET 10).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/BuildingBlocks/ChargeDesk.BuildingBlocks/ChargeDesk.BuildingBlocks.csproj src/BuildingBlocks/ChargeDesk.BuildingBlocks/
COPY src/Core/ChargeDesk.Core/ChargeDesk.Core.csproj src/Core/ChargeDesk.Core/
COPY src/Cadastros/ChargeDesk.Cadastros/ChargeDesk.Cadastros.csproj src/Cadastros/ChargeDesk.Cadastros/
COPY src/Operacao/ChargeDesk.Operacao/ChargeDesk.Operacao.csproj src/Operacao/ChargeDesk.Operacao/
COPY src/Financeiro/ChargeDesk.Financeiro/ChargeDesk.Financeiro.csproj src/Financeiro/ChargeDesk.Financeiro/
COPY src/Host/ChargeDesk.Host.Api/ChargeDesk.Host.Api.csproj src/Host/ChargeDesk.Host.Api/

RUN dotnet restore src/Host/ChargeDesk.Host.Api/ChargeDesk.Host.Api.csproj

COPY src/ ./src/
RUN dotnet publish src/Host/ChargeDesk.Host.Api/ChargeDesk.Host.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Autor: Anderson Pereira Silva | 29/07/2026 | Fuso Brasília (host Azure é UTC).
RUN apt-get update \
    && apt-get install -y --no-install-recommends tzdata \
    && rm -rf /var/lib/apt/lists/*
ENV TZ=America/Sao_Paulo

RUN mkdir -p /data/backups

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DB_PATH=/data/platform.db

EXPOSE 8080

ENTRYPOINT ["dotnet", "ChargeDesk.Host.Api.dll"]
