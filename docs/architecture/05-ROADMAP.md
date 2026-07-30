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

1. [x] Core (Empresa, Unidade, Usuário, Auth, Permissão básica, Licença)  
2. [x] Cadastros (Cliente, Veículo, Equipamento/Carregador)  
3. [x] Atendimento + especialização Carregamento  
4. [x] Disponibilidade de equipamento (ex-ponto)  
5. [x] Cliente herdado do veículo  
6. [x] Caixa (abrir/fechar, bloqueio com atendimento ativo)  
7. [x] Ticket / impressão (HTML navegador)  
8. [x] Horário América/São_Paulo  
9. [ ] Persistência durable (Azure Files / volume) — aguarda reauth Azure  
10. [x] Painel operacional mínimo (refresh ao vivo + histórico caixa)  

**Critério de saída:** checklist de paridade 07-MAPA-MIGRACAO 100% verde + testes automatizados das regras críticas.

---

## Fase 2 — Estacionamento (prova do modelo)

- [x] Tipo Atendimento = Estacionamento  
- [x] Extensão vaga / entrada / saída / permanência  
- [x] Tabela de preço por tempo (`CobrancaService.EstacionamentoPadrao`)  
- [x] Mapa de ocupação (SPA)  

**Critério de saída:** mesmos Core/Cadastros/Financeiro sem duplicar Cliente/Veículo.

---

## Fase 3 — Agenda + Ordens de Serviço

- [x] Agenda / reserva (`AgendaReserva` + check-in → Atendimento)  
- [x] OS com checklist/itens (`OrdemServico` + `OrdemServicoItem`)  
- [ ] Fotos anexas (próximo incremento)  

---

## Endurecimento / Fase 1.1

- [x] Solution `.sln` no repositório  
- [x] Auth JWT + policy Admin  
- [x] Testes de integração API  
- [x] Importação SQLite legado (`LegacySqliteImportService` + `scripts/importar-chargedesk.ps1`)  

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
