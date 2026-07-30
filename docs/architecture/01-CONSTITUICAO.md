# 01 — Constituição da ChargeDesk Platform

<!-- Autor: Anderson Pereira Silva -->
<!-- Data: 29/07/2026 -->
<!-- Descrição: Regras imutáveis da arquitetura modular. -->

## Filosofia

A ChargeDesk Platform permanece organizada durante toda a vida útil.  
Toda implementação respeita **baixo acoplamento**, **alta coesão** e **reutilização**.  
Nenhuma solução pode comprometer a evolução futura.

---

## Regra Nº 1 — Nenhum módulo conhece outro módulo

**Proibido:** `Carregamento → Financeiro → Caixa → Relatórios` com chamadas diretas.

**Obrigatório:** publicar evento / usar contrato do Core / BuildingBlocks.

```
Módulo A → Evento (Core/Bus) → Módulo B
```

---

## Regra Nº 2 — Nunca acessar banco de outro domínio

**Proibido:** Estacionamento ler tabela privada de Carregamento.

**Obrigatório:** Service, Interface, API pública do domínio ou Evento.

---

## Regra Nº 3 — Todo módulo é instalável / licenciável

Empresa A: só Carregamento.  
Empresa B: Carregamento + Estacionamento + Financeiro.  
Empresa C: só Estacionamento.

Todos funcionam sem código dos módulos não licenciados (feature flag + composição).

---

## Regra Nº 4 — Toda funcionalidade tem um proprietário

| Informação | Dono |
|------------|------|
| Cliente, Veículo, Equipamento | Cadastros |
| Empresa, Usuário, Permissão, Licença | Core |
| Atendimento | Operação |
| Caixa, Recebimento, Fatura | Financeiro |
| Relatórios | Relatórios (só leitura via Services) |

---

## Regra Nº 5 — Nunca duplicar entidades

Um único `Cliente`, um único `Veículo`, um único `Atendimento`.  
Proibido: `ClienteCarregamento`, `ClienteEstacionamento`.

---

## Regra Nº 6 — Nunca duplicar regras

Cálculo de permanência, energia, tarifa, ticket e caixa existe **em um único Service**.  
Frontend, API e relatório consomem a mesma regra.

---

## Regra Nº 7 — Frontend sem regra de negócio

Frontend: exibir, ocultar, validar formato de campo, consumir API.  
Regras de negócio, autorização, precificação e máquina de estados: **somente API/domínio**.

---

## Regra Nº 8 — Alteração importante gera evento

Exemplos: `ClienteCriado`, `AtendimentoIniciado`, `AtendimentoFinalizado`, `PagamentoRecebido`, `CaixaFechado`.  
Eventos são versionados (`*.v1`), imutáveis após publicação, processados com idempotência.

---

## Regra Nº 9 — Tudo gera auditoria

Cadastrar, editar, inativar, cancelar, receber, fechar caixa, trocar senha, alterar permissão, licença e configuração.

---

## Regra Nº 10 — Soft delete (não apagar operação)

Registro permanece; muda `Status` / `ExcluidoEm` / `ExcluidoPor`.  
Exceção documentada: requisitos legais de anonimização (LGPD) — ver Filosofia.

---

## Multiempresa e unidade (obrigatório)

Toda entidade operacional possui:

- `EmpresaId` (obrigatório)
- `UnidadeId` quando a operação for local (Atendimento, Caixa, Equipamento, Agenda)

Isolamento por empresa é invariante de segurança: token + empresa + permissão + licença **antes** de qualquer operação.

---

## Padrão de módulo

```
Modulo/
  Application/
  Domain/
  Infrastructure/
  Api/            (endpoints do host ou controllers)
  Tests/
```

Frontend compartilha shell único; rotas por módulo licenciado.

---

## Padrão de entidade

Campos obrigatórios em toda tabela de negócio:

| Campo | Uso |
|-------|-----|
| `Id` | PK |
| `EmpresaId` | Isolamento SaaS |
| `Status` | Enum |
| `CriadoEm` / `CriadoPor` | Auditoria |
| `AtualizadoEm` / `AtualizadoPor` | Auditoria |
| `ExcluidoEm` / `ExcluidoPor` | Soft delete |
| `Versao` | Concorrência otimista |

---

## Padrão de API

REST, substantivos no plural:

- `GET/POST /api/clientes`
- `GET/PUT/PATCH /api/veiculos/{id}`
- `POST /api/atendimentos`

Proibido: `/api/getCliente`, `/api/SalvarCliente`.

---

## Padrão de status

Sempre `enum` tipado. Nunca `"A"`, `"OK"`, `"SIM"` como regra.

---

## Segurança mínima por request

1. Token válido  
2. Empresa do contexto  
3. Usuário ativo  
4. Permissão da ação  
5. Licença do módulo  
6. Status da entidade  

---

## Zero regressão

Funcionalidades já homologadas no ChargeDesk (carregamento, disponibilidade de pontos, cliente via veículo, caixa aberto, horário Brasília, persistência) **não podem regredir** na migração.
