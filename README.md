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
- Interface de terminal com tabelas, cores e menus navegáveis
- Validação de entrada em todas as etapas
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
> plano. Se a tela "congelar" sem aceitar dígitos, é porque um container antigo
> ficou rodando solto:
> ```bash
> docker compose down        # derruba containers antigos
> docker compose up --build  # sobe de novo (agora interativo)
> ```
> Alternativa que sempre anexa o stdin:
> ```bash
> docker compose run --rm app
> ```
> Se o app for executado sem terminal (Ex.: redirecionamento de entrada), ele
> encerra com a mensagem *"Entrada desconectada (EOF)"* em vez de travar — sem TTY
> o app não tem para onde ler os números da conversão.

### Sem Docker

```bash
dotnet run --project src/TrocaMoedas.Presentation
```

### Variável de Ambiente (opcional)

Para usar taxas em tempo real, configure sua API key:

```bash
export EXCHANGE_RATE_API_KEY=sua-chave-aqui
```

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

### Sprint 2 — Cache & Testes
- [ ] Projeto de testes (xUnit + Moq)
- [ ] Testes unitários: Models, Services, Commands
- [ ] Testes de integração: SQLite
- [ ] Cache de taxas (1h TTL)
- [ ] Cobertura 80%+

### Sprint 3 — Deploy & CI/CD
- [ ] Dockerfile multi-stage produção
- [ ] docker-compose.prod.yml
- [ ] GitHub Actions CI (build + test)
- [ ] Validação final MVP

Veja as [issues detalhadas](https://github.com/OsirisMariano/troca-moedas/issues) para cada tarefa.

## Licença

Projeto open source para fins educacionais e profissionais.
