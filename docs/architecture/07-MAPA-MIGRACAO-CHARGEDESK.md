# 07 — Mapa de Migração ChargeDesk → Platform

<!-- Autor: Anderson Pereira Silva -->
<!-- Data: 29/07/2026 -->
<!-- Descrição: Paridade funcional e mapeamento do sistema carregamento-eletrico. -->

## Origem

Repositório de referência: `C:\Projetos\carregamento-eletrico` (ChargeDesk).  
Estratégia: **fork evolutivo** — DNA de negócio preservado; estrutura de domínios nova.

---

## Conceitos

| Atual | Platform |
|-------|----------|
| Sessão de carregamento | `Atendimento` + `AtendimentoCarregamento` |
| Ponto de carregamento | `Equipamento` (Tipo = Carregador) |
| Cliente / Veículo | Cadastros (mesma regra: sessão/atendimento herda cliente do veículo) |
| Caixa / movimentações | Financeiro.Caixa |
| Ticket | Campo/numéracao no Atendimento |
| PrintAgent / fila impressão | Core.Impressao (Fase 1) |
| Dashboard operacional | Relatórios / Painel (leitura) |
| SQLite local / Azure `/data` | Persistência durable obrigatória |

---

## Regras críticas — não regredir

Checklist de paridade Fase 1:

| # | Regra | Origem | Status Platform |
|---|-------|--------|-----------------|
| 1 | Nova carga exige caixa aberto | SessaoValidacao / API | OK |
| 2 | Ponto/equipamento ocupado = indisponível | `/pontos/disponiveis` + POST | OK |
| 3 | Lista vazia de disponíveis ≠ fallback para todos | app.js regressão | OK |
| 4 | Cliente da sessão = dono do veículo | `ResolverClienteIdDaSessao` | OK |
| 5 | Telefone obrigatório no cliente | Cadastro cliente | OK |
| 6 | Fechar caixa bloqueado com atendimento EmExecucao | Validação caixa | OK |
| 7 | Horário operacional Brasília | `HorarioOperacional` | OK |
| 8 | Volume persistente Azure `/data` | deploy + Azure Files | Pendente (reauth) |
| 9 | Ticket único | TicketNumeracao | OK |
| 10 | Valor/tempo ao vivo em andamento | CobrancaService + DTO | OK |

---

## Fluxo alvo (Carregamento)

```
Cliente → Veículo → (Caixa aberto) → Atendimento(Carregamento)
  → Equipamento livre → EmExecucao → Finalizar
  → AguardandoPagamento → Recebimento → Finalizado
```

UI: operador escolhe **Veículo**; Cliente/Telefone somente leitura.

---

## O que NÃO migrar como está

- Acoplamento frontend com fallbackes inseguros  
- Estrutura monolítica sem boundaries de domínio  
- Entidade “Sessão” como raiz do modelo (vira Atendimento)  
- Scripts/temp locais não versionados  

---

## Plano técnico sugerido

1. Esqueleto modular (este repo)  
2. Portar Domain Services de cobrança/validação como bibliotecas de Operacao/Financeiro  
3. API Host com autenticação Core  
4. SPA reorganizada por rotas de módulo  
5. Script de importação SQLite legado → novo schema (Fase 1.1)  
6. Cutover Azure com volume já existente quando possível  

---

## Critério de aceite da migração

Operador consegue, no Platform, o mesmo dia de trabalho do ChargeDesk atual:  
abrir caixa, cadastrar cliente/veículo, iniciar/finalizar carga, receber, fechar caixa, imprimir ticket — **sem regressões da tabela acima**.
