# 📋 Plano de Sprints - Conversor de Moedas Pro (v2.0)

> **Product Owner**: Análise e planejamento criados em 2026-08-20
> **Última sincronização**: 2026-11-12 — Sprint 3 concluída e MVP validado (issue #22)
> **Baseado no**: docs/PRD.md
> **Status do Projeto**: Clean Architecture concluída; Sprint 1, 2 e 3 concluídas (128 de 128 SP); **MVP aprovado**
> **Arquitetura**: Clean Architecture (4 projetos separados)
> **Ordem**: Cronológica - siga de cima para baixo para evitar quebras

---

## 📊 Análise de Produto

### Estado Atual: Sprint 1 Completa e VALIDADA ✅

#### ✅ Build Validado
- Build sem errors via `docker compose build`
- Sistema inicializa e exibe menu
- Inputs via stdin funcionam corretamente
- Conversões salvas com sucesso no SQLite
- Histórico persistido e consultável

#### ✅ Testes Realizados
1. **✅ Build**: Compila sem erros críticos
2. **✅ Startup**: Menu exibido com formatação Spectre
3. **✅ Conversão**: BRL → USD funcionando (R$100 → $500, taxa 5.0)
4. **✅ Persistência**: Conversões salvas no SQLite (`/app/data/conversor.db`)
5. **✅ Histórico**: Consulta retorna dados salvados com data/hora correta
6. **✅ Interatividade**: `Console.ReadLine()` recebe inputs via Docker stdin

#### 🛠️ Correções Aplicadas
- `Path.GetDirectory` → `Path.GetDirectoryName`
- `reader.GetInt32("id")` → `reader.GetInt32(reader.GetOrdinal("id"))`
- `AnsiConsole.Prompt()` → `Console.ReadLine()` + validação manual
- Removido `System.Net.Http.Json` redundante
- Atualizado `Microsoft.Extensions.DependencyInjection`

#### 🔧 Arquitetura Implementada

```
src/
├── TrocaMoedas.Domain/          # ✅ CONCLUÍDO
│   └── Models/
│       ├── Currency.cs          # Entidade com enum e extensions
│       ├── Conversion.cs        # Record puro
│       └── ExchangeRate.cs      # Record puro com lógica de taxa
├── TrocaMoedas.Application/     # ✅ CONCLUÍDO
│   ├── Services/
│   │   └── IExchangeRateService.cs  # Interface
│   ├── Repositories/
│   │   └── IConversionRepository.cs # Interface
│   └── DTOs/
│       └── ConversionResult.cs        # DTO
├── TrocaMoedas.Infrastructure/  # ✅ CONCLUÍDO
│   ├── Services/
│   │   ├── ExchangeRateApiService.cs    # API call
│   │   └── FallbackExchangeRateService.cs # Fallback
│   ├── Data/
│   │   ├── DatabaseInitializer.cs       # ✅ CRIA TABELAS
│   │   └── Repositories/
│   │       └── SqliteConversionRepository.cs # ✅ CRUD
│   └── Configuration/
│       └── AppConfig.cs                 # ✅ Config persistida
└── TrocaMoedas.Presentation/   # ✅ CONCLUÍDO
    ├── Program.cs                       # ✅ DI Setup
    └── Commands/
        ├── MenuCommand.cs               # ✅ Menu navegável
        ├── ConvertCommand.cs            # ✅ Salva conversão ✅
        └── HistoryCommand.cs            # ✅ Consulta histórico
```

### 📋 Ordem Cronológica de Implementação (siga esta sequência)

Esta lista está organizada **cronologicamente e por dependências técnicas**. 
Implemente de cima para baixo para evitar quebras e garantir que cada tarefa
tenha suas dependências já atendidas.

---

### ✅ Sprint 1 CONCLUÍDA (Arquitetura Clean Architecture)

A migração para Clean Architecture foi implementada com sucesso:

#### 🔢 Ordem de Execução (concluída):

| Ordem | ID | Tarefa | SP | Status | Critério de Aceite |
|-------|----|--------|-----|--------|-------------------|
| 1️⃣ | P-01-01 | Criar estrutura Clean Architecture (4 projetos) | 8 | ✅ | Domain, Application, Infrastructure, Presentation separados |
| 2️⃣ | P-01-02 | Criar `DatabaseInitializer` | 5 | ✅ | Cria tabelas `conversoes`, `configuracoes` no SQLite |
| 3️⃣ | P-01-03 | Criar `IConversionRepository` + `SqliteConversionRepository` | 8 | ✅ | Implementa `SaveAsync`, `GetRecentAsync`, `ClearAsync` |
| 4️⃣ | P-01-04 | Integrar repositório no `ConvertCommand` | 3 | ✅ | Salva conversão no SQLite após sucesso |
| 5️⃣ | P-01-05 | Criar `HistoryCommand` | 5 | ✅ | Mostra histórico em tabela Spectre |
| 6️⃣ | P-01-06 | Criar `AppConfig` (Configuration) | 5 | ✅ | Persiste configurações em SQLite |
| 7️⃣ | P-01-07 | Configurar DI em `Program.cs` | 5 | ✅ | Injeção de dependências total |
| 8️⃣ | P-01-08 | Atualizar Docker/docker-compose/README | 3 | ✅ | Novo projeto configurado |
| 9️⃣ | P-01-09 | Corrigir interatividade Docker (Console.ReadLine) | 5 | ✅ | Menu recebe inputs via Docker TTY |
| 🔟 | P-01-10 | Validar build + execução | 3 | ✅ | Sistema inicia e responde a inputs |
| 1️⃣1️⃣ | P-01-11 | Corrigir erros de compilação (Path.GetDirectory, reader.GetOrdinal) | 3 | ✅ | Build sem errors |
| 1️⃣2️⃣ | P-01-12 | Testes manuais de fluxo completo | 3 | ✅ | Conversão + histórico funcionando |

**Total Sprint 1**: 63 SP | **Status**: ✅ Concluída e Validada

**✅ Ordem seguida com sucesso!** Todas as dependências foram satisfeitas.

---

### 🔴 Sprint 2: Cache & Testes Unitários (Semana 2 do PRD)

**Objetivo**: Adicionar cache e testes automatizados (unitários)

#### 🔢 Ordem de Execução (priorizada):

| Ordem | ID | Tarefa | SP | Critério de Aceite |
|-------|----|--------|-----|-------------------|
| 1️⃣ | P-02-01 | Configurar projeto de testes + dependências (xUnit, Moq) | 3 | ✅ Projeto tests pronto (PR #26) |
| 2️⃣ | P-02-02 | Testes unitários - Models e Extensions | 5 | ✅ 23 testes (PR #27) |
| 3️⃣ | P-02-03 | Testes unitários - FallbackExchangeRateService | 5 | ✅ 9 testes + correção da inversão de taxas (PR #28) |
| 4️⃣ | P-02-04 | Testes unitários - ExchangeRateApiService (mockada) | 5 | ✅ 4 testes com HTTP mockado, sem chamada real (PR #28) |
| 5️⃣ | P-02-05 | Testes unitários - ConvertCommand (com mocks) | 5 | ✅ Lógica extraída para `ConversionService` + 4 testes com mocks (PR #29) |
| 6️⃣ | P-02-06 | Testes de integração - SqliteConversionRepository | 8 | ✅ 6 testes (SQLite em memória, cache compartilhado) (PR #32) |
| 7️⃣ | P-02-07 | Configurar cobertura de testes (80%+) | 3 | ✅ 91% de linhas (Domain/Application/Infrastructure) via `scripts/coverage.sh` |
| 8️⃣ | P-02-08a | Definir design do CacheService (escopo, TTL, limites) | 2 | ✅ Decisões registradas na issue #23 |
| 9️⃣ | P-02-08 | Implementar `CacheService` (1h cache) | 5 | ✅ Cache em memória por moeda base + TTL configurável (PR #43) |
| 🔟 | P-02-04a | Implementar backoff exponencial para rate limits da API | 3 | ✅ Retry 429/5xx + fallback automático, inclusive 403 (PR #44) |
| 1️⃣1️⃣ | P-02-06a | Implementar botão "Limpar histórico" na UI | 3 | ✅ Ação com confirmação no histórico (PR #42) |

**Total Sprint 2**: 49 SP | **Duração**: 7-10 dias | **Time**: 2 devs

**Progresso**: 49 de 49 SP concluídos. Sprint 2 finalizada.

**⚠️ Importante**: Testes vêm primeiro para validar código existente.
CacheService é último (menor prioridade técnica atual).

**🔧 Correção incluída na P-02-03**: `FallbackExchangeRateService.GetRatesAsync` tinha
semântica invertida (R$100 → $500 no fallback). As taxas agora seguem a convenção
"1 unidade da moeda = X da moeda base" e `GetRate`/`Convert` ficam consistentes.

**🏗️ Refactor da P-02-05**: a lógica de conversão que vivia dentro do
`ConvertCommand` (buscar taxas, calcular, salvar no SQLite) foi extraída para
`ConversionService` na camada Application. O command virou uma casca fina de UI,
e a lógica se tornou testável com mocks (issue #15).

**🖥️ P-02-05b (fix interatividade, PR #31)**: quando o app roda sem TTY/`stdin`
anexado (Ex.: container solto com `docker compose up`), `Console.ReadLine()`
retornava `null` e o menu entrava em loop infinito de "Opção inválida" — a tela
parecia congelada e não aceitava dígitos. Agora o EOF é detectado
(`ConsoleInput.IsEof`) e o app **encerra de forma limpa** no menu, no convert e
no histórico. Recomenda-se `docker compose down` antes de `up` e, como
alternativa garantida, `docker compose run --rm app`.

---

### ✅ Sprint 3: Infra & Deploy (Semana 4 do PRD)

**Objetivo**: Docker production e CI/CD — **concluída**

#### 🔢 Ordem de Execução:

| Ordem | ID | Tarefa | SP | Critério de Aceite | Status |
|-------|----|--------|-----|-------------------|--------|
| 1️⃣ | P-03-01 | Dockerfile multi-stage produção | 5 | Build otimizado, runtime slim | ✅ PR #47 |
| 2️⃣ | P-03-02 | docker-compose.prod.yml | 3 | Volume persiste DB, ambiente prod | ✅ PR #47 |
| 3️⃣ | P-03-03 | GitHub Actions CI workflow | 5 | Build + teste no push/PR, status badge | ✅ PR #48 |
| 4️⃣ | P-03-04 | Validação final MVP | 3 | Todos critérios de aceite do PRD validados | ✅ [mvp-validation.md](./mvp-validation.md) |

**Total Sprint 3**: 16 SP | **Duração**: 3-4 dias | **Time**: 1 dev + PO/QA

**Progresso**: 16 de 16 SP concluídos. Todas as issues (#19–#22) fechadas.

**⚠️ Importante**: Docker/CI só funcionam se todos os testes passarem (Sprint 2 concluída).

---

## 📈 Velocity & Forecasting

### Velocity Atual
- Sprint 1 (Arquitetura): 50% do backlog completo
- Velocity esperada: 20-25 SP por sprint (foco em testes agora)

**💡 Nota**: Velocity reduzida porque a Sprint 1 incluiu refatoração de arquitetura, mas agora permite desenvolvimento mais rápido e testável.

---

### Forecast MVP (ordem cronológica)

| Ordem | Sprint | Semana | SP Planejado | Conclusão | Status |
|-------|--------|--------|-------------|-----------|--------|
| ✅ | Sprint 1 | Semana 1-3 | 63 SP | Clean Architecture + SQLite + Histórico + Validação | Concluída |
| ✅ | Sprint 2 | Semana 2 | 49 SP | Cache + Testes Unitários + Decisões de Design | Concluída |
| ✅ | Sprint 3 | Semana 4 | 16 SP | Docker + CI/CD + validação do MVP | Concluída |

**MVP Completo**: entregue e validado em 2026-11-12 (128 SP).

### 📈 Velocity Atual
- Sprint 1 (Arquitetura + Interatividade): 63 SP completo
- Velocity esperada: 20-25 SP por sprint (foco em testes agora)

---

## ❓ Decisões Pendentes (convertidas em issues)

Todas as decisões da Sprint 2 foram resolvidas e as issues fechadas:

1. **CacheService design** → [Issue #23](https://github.com/OsirisMariano/troca-moedas/issues/23) — ✅ fechada (cache por moeda base, em memória, TTL configurável).
2. **Rate limits API** → [Issue #25](https://github.com/OsirisMariano/troca-moedas/issues/25) — ✅ fechada (retry 429/5xx + fallback).
3. **Limpar histórico UI** → [Issue #24](https://github.com/OsirisMariano/troca-moedas/issues/24) — ✅ fechada (ação com confirmação).

---

## 📍 Critérios de Aceite do MVP (Checklist Final)

### Estado dos Critérios de Aceite do MVP

- [x] Conversão entre BRL e 5+ moedas com taxa real ✅ **VALIDADO**
- [x] Interface de menu com Spectre.Console (setas, tabelas) ✅ **VALIDADO**
- [x] Histórico persistido em SQLite e consultável ✅ **VALIDADO**
- [x] Testes unitários passando com 80%+ cobertura (atual: 91,72% em Domain/Application/Infrastructure) ✅ **VALIDADO**
- [x] `docker compose up --build` funcional (dev) ✅ **VALIDADO**
- [x] `docker compose -f docker-compose.prod.yml up --build` funcional (prod) ✅ **VALIDADO**
- [x] CI verde no GitHub Actions ✅ **VALIDADO**

> Evidências em [docs/mvp-validation.md](./mvp-validation.md).

---

## 📺 Como Testar (Guia do Usuário)

### Passo 1: Iniciar o Sistema
```bash
cd /home/pc/Desktop/Project/troca-moedas
docker compose up --build
```

### Passo 2: Usar Interativamente
Digite as opções diretamente no terminal:
- `1` → Converter moeda (escolha BRL→USD, digite 100)
- `2` → Ver histórico de conversões
- `3` → Configurações (não implementada)
- `4` → Sair

### Passo 3: Parar o Sistema
```bash
# No terminal: Ctrl+C
docker compose down -v
```

### 🔍 Verificar Persistência
```bash
docker compose exec app bash -c "apt install -y sqlite3 && sqlite3 /app/data/conversor.db 'SELECT * FROM conversoes;'"
```

---

## ⚠️ Dependências e Bloqueios Atuais

| Tarefa | Bloqueia | Bloqueada Por |
|--------|----------|---------------|
| CacheService | ExchangeRateApiService com cache | ✅ Concluído na Sprint 2 |
| Testes unitários - Services | CacheService implementado | ✅ Concluído na Sprint 2 |
| Docker prod | Testes passando | ✅ Concluído na Sprint 3 |
| CI/CD | Docker prod + Testes | ✅ Concluído na Sprint 3 |

**💡 Dica**: Arquitetura concluída removeu a maior parte dos bloqueios. Foco agora em testes e infra.