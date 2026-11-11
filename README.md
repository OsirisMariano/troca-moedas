# Troca Moedas

Conversão de moedas direto do terminal — rápido, prático e containerizado.

## Sobre

Ferramenta de linha de comando para converter valores entre diferentes moedas com taxas de câmbio atualizadas. Interface interativa, validação de entrada e total suporte a Docker.

## Moedas Suportadas

| Moeda | Código | Nome             |
|-------|--------|------------------|
| R$    | BRL    | Real Brasileiro  |
| $     | USD    | Dólar Americano  |
| €     | EUR    | Euro             |
| £     | GBP    | Libra Esterlina  |
| ¥     | JPY    | Iene Japonês     |
| $     | ARS    | Peso Argentino   |

## Funcionalidades

- Taxas de câmbio em tempo real via API, com fallback automático
- Interface de terminal com tabelas, cores e menu navegável pelas setas
- Validação de entrada em todas as etapas
- Valores aceitam ponto ou vírgula decimal (`100.50` ou `100,50`)
- Suporte a conversão entre quaisquer duas moedas da lista
- Ambiente 100% containerizado com Docker

## Stack Tecnológica

| Camada        | Tecnologia                          |
|---------------|--------------------------------------|
| Runtime       | .NET 10                              |
| Linguagem     | C#                                   |
| UI Terminal   | Spectre.Console                      |
| API de Câmbio | ExchangeRate-API                     |
| Container     | Docker + docker-compose              |
| Arquitetura   | Clean Architecture (4 camadas)       |
| DI Container  | Microsoft.Extensions.DependencyInjection |

## Como Rodar

### Com Docker (recomendado)

```bash
docker compose up --build
```

> **Interatividade:** rode em um terminal real e mantenha o app em primeiro
> plano. Se a tela ficar em branco ou "congelar" sem aceitar dígitos:
>
> 1. Derrube containers antigos e suba de novo:
>    ```bash
>    docker compose down
>    docker compose up --build
>    ```
> 2. Se continuar travado, rode anexando o stdin:
>    ```bash
>    docker compose run --rm app
>    ```
> 3. Se a tela ainda não renderizar, o Compose pode ter criado o TTY do
>    container com tamanho `0x0`. O app detecta isso e usa 80 colunas, mas a
>    opção mais confiável é rodar direto com Docker, que propaga o tamanho do
>    terminal:
>    ```bash
>    docker run --rm -it \
>      -v "$PWD":/app \
>      -v troca-moedas_app-data:/app/data \
>      -w /app \
>      -e TERM=xterm-256color \
>      mcr.microsoft.com/dotnet/sdk:10.0 \
>      dotnet run --project src/TrocaMoedas.Presentation
>    ```
>
> Se o app for executado sem terminal (Ex.: redirecionamento de entrada), o menu
> troca automaticamente para o modo numérico e o app encerra com a mensagem
> *"Entrada desconectada (EOF)"* em vez de travar.
>
> **Menu:** em um terminal interativo, use as **setas ↑/↓** e **Enter** para
> escolher a opção. Quando a entrada é redirecionada (pipe/CI), digite o número
> da opção e pressione Enter.

### Sem Docker

> Requer o **.NET SDK 10** instalado (o projeto tem como alvo `net10.0`).

```bash
dotnet run --project src/TrocaMoedas.Presentation
```

### Variável de Ambiente (opcional)

Para usar taxas em tempo real, configure sua API key:

```bash
export EXCHANGE_RATE_API_KEY=sua-chave-aqui
```

> O `docker-compose.yml` repassa essa variável automaticamente para o container.
> Também é possível rodar:
> ```bash
> EXCHANGE_RATE_API_KEY=sua-chave-aqui docker compose up --build
> ```
>
> Se não configurada, o sistema usa taxas de fallback automaticamente.

## Estrutura do Projeto

```
troca-moedas/
├── Dockerfile.dev
├── docker-compose.yml
├── src/
│   ├── TrocaMoedas.Domain/              # Entidades (sem dependências)
│   │   └── Models/
│   │       ├── Currency.cs
│   │       ├── Conversion.cs
│   │       └── ExchangeRate.cs
│   ├── TrocaMoedas.Application/         # Interfaces e DTOs
│   │   ├── Services/
│   │   │   └── IExchangeRateService.cs
│   │   ├── Repositories/
│   │   │   └── IConversionRepository.cs
│   │   └── DTOs/
│   │       └── ConversionResult.cs
│   ├── TrocaMoedas.Infrastructure/    # Implementações
│   │   ├── Services/
│   │   │   ├── ExchangeRateApiService.cs
│   │   │   └── FallbackExchangeRateService.cs
│   │   ├── Data/
│   │   │   ├── DatabaseInitializer.cs
│   │   │   └── Repositories/
│   │   │       └── SqliteConversionRepository.cs
│   │   └── Configuration/
│   │       └── AppConfig.cs
│   └── TrocaMoedas.Presentation/      # UI e Orquestração
│       ├── Program.cs
│       └── Commands/
│           ├── MenuCommand.cs
│           ├── ConvertCommand.cs
│           └── HistoryCommand.cs
├── tests/
│   └── CurrencyConverter.Tests/
└── docs/
    ├── PRD.md
    └── sprint-plan.md
```

## Roadmap

### Sprint 1 — Fundação ✅
- [x] Clean Architecture (4 projetos)
- [x] Conversão multi-moeda com API real
- [x] Persistência SQLite (histórico de conversões)
- [x] Interface terminal com Spectre.Console
- [x] Docker dev (hot reload)

### Sprint 2 — Cache & Testes (em andamento)
- [x] Projeto de testes (xUnit + Moq)
- [x] Testes unitários: Models, Services, Commands
- [x] Testes de integração: SQLite
- [ ] Cache de taxas (1h TTL)
- [ ] Cobertura 80%+ (atual: 44,8%)

### Sprint 3 — Deploy & CI/CD
- [ ] Dockerfile multi-stage produção
- [ ] docker-compose.prod.yml
- [ ] GitHub Actions CI (build + test)
- [ ] Validação final MVP

Veja as [issues detalhadas](https://github.com/OsirisMariano/troca-moedas/issues) para cada tarefa.

## Licença

Projeto open source para fins educacionais e profissionais.
