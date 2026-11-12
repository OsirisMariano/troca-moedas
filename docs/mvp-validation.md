# ✅ Validação Final do MVP

> **Data**: 2026-11-12
> **Issue**: [#22 — P-03-04](https://github.com/OsirisMariano/troca-moedas/issues/22)
> **Ambiente**: Linux, Docker 28.x, Docker Compose 5.5.0, .NET SDK 10
> **Resultado**: **aprovado** — 7 de 7 critérios de aceite do [PRD](./PRD.md#7-critérios-de-aceite-mvp)

---

## Resumo

| # | Critério | Status | Evidência |
|---|----------|--------|-----------|
| 1 | Conversão entre BRL e 5+ moedas com taxa real | ✅ | 6 moedas suportadas; `ExchangeRateApiService` integrado à ExchangeRate-API; parsing coberto por testes com payload real |
| 2 | Interface de menu com Spectre.Console (setas, tabelas) | ✅ | Menu navegável por setas no TTY, modo numérico em pipe e tabelas de histórico |
| 3 | Histórico persistido em SQLite e consultável | ✅ | 2 conversões gravadas e listadas; `conversor.db` no volume |
| 4 | Testes unitários passando com 80%+ cobertura | ✅ | **135 testes**, **91,72% de linhas** |
| 5 | `docker compose up --build` funcional (dev) | ✅ | Menu renderizado e fluxo completo via `run --rm -T app` |
| 6 | `docker compose -f docker-compose.prod.yml up --build` funcional (prod) | ✅ | Build multi-stage, usuário não-root e volume nomeado validados |
| 7 | CI verde no GitHub Actions | ✅ | Workflow [CI](https://github.com/OsirisMariano/troca-moedas/actions/workflows/ci.yml) passando; check obrigatório em `develop` e `main` |

---

## 1. Conversão entre moedas

Moedas suportadas: **BRL, USD, EUR, GBP, JPY e ARS** (base + 5 destinos).

Conversões executadas no ambiente de produção:

| Origem | Destino | Valor | Resultado | Taxa |
|--------|---------|-------|-----------|------|
| BRL | USD | R$ 100,50 | $ 20,10 | 0,2000 |
| BRL | EUR | R$ 50,00 | € 9,26 | 0,1852 |
| BRL | JPY | R$ 1.000,00 | ¥ 29.411,76 | 29,4118 |

> O caminho de taxa em tempo real (`GET /v6/{key}/latest/{base}`) está implementado e
> coberto por testes com o formato de payload da API. Os smokes acima usaram o
> fallback porque o ambiente não possui uma `EXCHANGE_RATE_API_KEY` válida; o fluxo
> de conversão é o mesmo. Com a chave configurada, as taxas vêm da API e ficam em
> cache por 1 hora.

## 2. Interface

- Menu principal, conversão e histórico com cabeçalho, cores e tabelas do Spectre.Console.
- TTY interativo: navegação por **setas ↑/↓ + Enter**.
- Pipe/CI: seleção numérica e Enter, sem travar.
- Histórico em tabela com data, origem, destino e taxa.

## 3. Persistência

- SQLite em `/app/data/conversor.db`.
- Conversões gravadas e recuperadas na tela de histórico.
- No container de produção o arquivo pertence a `1654:1654` (usuário `app`).
- Volume nomeado `troca-moedas-prod-data` mantém os dados entre execuções.

## 4. Testes e cobertura

```
dotnet test → 135 testes, 0 falhas
Cobertura (Domain + Application + Infrastructure): 91,72% de linhas
Limite mínimo: 80%
```

Cobertura por camada: Domain 100%, Application 100%, Infrastructure ~89%.
A camada `Presentation` (UI de terminal) fica fora do cálculo, conforme decisão da Sprint 2.

## 5. Docker de desenvolvimento

```bash
docker compose up --build          # menu interativo
docker compose run --rm -T app     # execução por pipe
```

Fluxo validado: duas conversões (USD e EUR), histórico com 2 registros e saída limpa.
Em `docker compose up` com pipe, o `tty: true` do serviço impede o encaminhamento do
stdin; para automação use `docker compose run --rm -T app` (documentado no README).

## 6. Docker de produção

```bash
docker compose -f docker-compose.prod.yml up --build
docker compose -f docker-compose.prod.yml run --rm -T app
```

- Build multi-stage `sdk:10.0` → `runtime:10.0`.
- Imagem final: **349 MB**, sem SDK nem artefatos de build.
- Processo roda como `uid=1654(app)`.
- Volume nomeado grava o SQLite com o mesmo owner.
- Conversões BRL→USD e BRL→JPY, histórico e saída validados.

## 7. CI

- Workflow `.github/workflows/ci.yml` em push/PR para `develop` e `main`.
- Restore, build `Release`, testes com cobertura e limite de 80%.
- Relatório publicado como artefato.
- Execução validada: check **“Build, testes e cobertura”** verde.
- Branch protection em `develop` e `main` exige o check antes do merge.

---

## Limitações conhecidas

- **API key**: sem `EXCHANGE_RATE_API_KEY` válida o app usa fallback (comportamento esperado).
- **TTY `0x0`**: o Compose pode criar o TTY sem dimensões; o app assume 80 colunas. Para uso real, rode anexado em um terminal.
- **`NU1903`**: aviso pré-existente de vulnerabilidade em `SQLitePCLRaw.lib.e_sqlite3 2.1.10`; não bloqueia o MVP.
- **`restart`**: o compose de produção usa `restart: "no"` de propósito, pois é um CLI interativo (reinício automático reabriria o app em loop).

---

## Como reproduzir

```bash
# testes e cobertura
./scripts/coverage.sh

# dev (interativo)
docker compose up --build

# dev (automação)
printf '1\n1\n1\n100,50\n\n2\n1\n\n4\n' | docker compose run --rm -T app

# prod
docker compose -f docker-compose.prod.yml up --build

# CI local (mesmos passos do workflow)
dotnet restore troca-moedas.sln
dotnet build troca-moedas.sln -c Release --no-restore
dotnet test tests/CurrencyConverter.Tests/CurrencyConverter.Tests.csproj -c Release --no-build
```
