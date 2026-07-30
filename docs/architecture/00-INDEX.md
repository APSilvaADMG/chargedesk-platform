# 00 — Índice da Constituição

<!-- Autor: Anderson Pereira Silva -->
<!-- Data: 29/07/2026 -->
<!-- Descrição: Mapa oficial da arquitetura ChargeDesk Platform. -->

Este conjunto de documentos é a **Constituição** da ChargeDesk Platform.  
Qualquer implementação (humana ou assistida) deve respeitá-los. Em caso de conflito entre código e Constituição, **a Constituição prevalece** até revisão formal.

## Ordem de leitura

1. [01 — Constituição](01-CONSTITUICAO.md) — regras absolutas
2. [02 — Filosofia](02-FILOSOFIA.md) — pilares e princípios
3. [03 — Modelo de Domínio](03-DOMINIO.md) — entidades oficiais
4. [04 — Modelo Físico Fase 1](04-MODELO-FISICO-FASE1.md) — MVP de dados
5. [05 — Roadmap](05-ROADMAP.md) — o que construir e quando
6. [06 — Produtos / Soluções](06-PRODUTOS-SOLUCOES.md) — o que se vende
7. [07 — Migração ChargeDesk](07-MAPA-MIGRACAO-CHARGEDESK.md) — herança do sistema atual
8. [08 — Anti-padrões](08-REGRAS-ANTIPADROES.md) — o que nunca fazer

## Hierarquia de decisão

1. Constituição (estes docs)
2. Zero regressão operacional (paridade ChargeDesk nas funcionalidades migradas)
3. Segurança / integridade / LGPD
4. Escalabilidade e reutilização
5. Preferência técnica / estilo / quality gate

## Processo de alteração da Constituição

- Qualquer mudança nestes documentos exige decisão explícita do dono do produto.
- Alteração de regra maior (Regra Nº 1–10) é mudança **Major**.
- Novas entidades no Domain Model exigem atualização do doc 03 **antes** do código.
