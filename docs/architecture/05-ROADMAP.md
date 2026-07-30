# 05 — Roadmap

<!-- Autor: Anderson Pereira Silva -->
<!-- Data: 29/07/2026 -->
<!-- Descrição: Fases de entrega da ChargeDesk Platform. -->

## Estratégia

**Fork evolutivo** a partir do ChargeDesk (`carregamento-eletrico`):  
reaproveitar regras homologadas; reorganizar em domínios; **não** reescrever comportamento operacional sem necessidade.

Arquitetura: **modular monolith** (.NET) + SPA. Microserviços só se métrica/necessidade exigir.

---

## Fase 0 — Fundação (atual)

- [x] Constituição e filosofia  
- [x] Modelo de domínio  
- [x] Modelo físico Fase 1  
- [x] Roadmap e produtos  
- [x] Mapa de migração  
- [x] Esqueleto do repositório  

**Critério de saída:** docs oficiais aprovados pelo dono do produto.

---

## Fase 1 — Paridade Carregamento

Objetivo: substituir o ChargeDesk operacional pelo Platform **sem perda** das regras críticas.

1. Core (Empresa, Unidade, Usuário, Auth, Permissão básica, Licença)  
2. Cadastros (Cliente, Veículo, Equipamento/Carregador)  
3. Atendimento + especialização Carregamento  
4. Disponibilidade de equipamento (ex-ponto)  
5. Cliente herdado do veículo  
6. Caixa (abrir/fechar, bloqueio com atendimento ativo)  
7. Ticket / impressão  
8. Horário América/São_Paulo  
9. Persistência durable (Azure Files / volume)  
10. Painel operacional mínimo  

**Critério de saída:** checklist de paridade 07-MAPA-MIGRACAO 100% verde + testes automatizados das regras críticas.

---

## Fase 2 — Estacionamento (prova do modelo)

- Tipo Atendimento = Estacionamento  
- Extensão vaga / entrada / saída / permanência  
- Tabela de preço por tempo  
- Mapa de ocupação  

**Critério de saída:** mesmos Core/Cadastros/Financeiro sem duplicar Cliente/Veículo.

---

## Fase 3 — Agenda + Ordens de Serviço

- Agenda / reserva  
- OS com checklist, itens, fotos  

---

## Fase 4 — Financeiro corporativo + CRM

- Contas a receber/pagar, mensalidade, fatura  
- CRM leve (leads, histórico de contato)  
- Comunicação (e-mail/WhatsApp)  

---

## Fase 5 — Capabilities + Campos dinâmicos

- Motor de capacidades  
- Campos personalizados por empresa  

---

## Versionamento

Semantic Versioning:

- **Major** — breaking (API/domínio)  
- **Minor** — novo módulo/feature compatível  
- **Patch** — correção  

Constituição Major = revisão explícita dos docs 01–03.
