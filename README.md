# ChargeDesk Platform

<!-- Autor: Anderson Pereira Silva -->
<!-- Data: 29/07/2026 -->
<!-- Descrição: Plataforma modular de mobilidade, eletropostos, estacionamento e serviços automotivos. -->

Plataforma corporativa para gestão de **eletropostos**, **estacionamentos**, **lava-rápidos**, **oficinas**, **valet**, **frotas** e demais operações automotivas — com a mesma infraestrutura de autenticação, cadastros, atendimento, financeiro, auditoria e segurança.

## Origem

Fork evolutivo do sistema **ChargeDesk** (`carregamento-eletrico`), preservando regras operacionais já homologadas (carregamento, caixa, ticket, pontos, horário Brasília, persistência Azure) e reorganizando a arquitetura em **domínios** com **Atendimento** no centro.

## Documentação oficial (Constituição)

Leitura obrigatória antes de qualquer implementação:

| # | Documento | Conteúdo |
|---|-----------|----------|
| 00 | [Índice](docs/architecture/00-INDEX.md) | Mapa da Constituição |
| 01 | [Constituição](docs/architecture/01-CONSTITUICAO.md) | Regras imutáveis e anti-padrões |
| 02 | [Filosofia](docs/architecture/02-FILOSOFIA.md) | Pilares e princípios |
| 03 | [Modelo de Domínio](docs/architecture/03-DOMINIO.md) | Entidades e relacionamentos |
| 04 | [Modelo Físico Fase 1](docs/architecture/04-MODELO-FISICO-FASE1.md) | Tabelas mínimas do MVP |
| 05 | [Roadmap](docs/architecture/05-ROADMAP.md) | Fases de entrega |
| 06 | [Produtos / Soluções](docs/architecture/06-PRODUTOS-SOLUCOES.md) | Bundles comercializáveis |
| 07 | [Migração ChargeDesk](docs/architecture/07-MAPA-MIGRACAO-CHARGEDESK.md) | Paridade com o sistema atual |
| 08 | [Anti-padrões](docs/architecture/08-REGRAS-ANTIPADROES.md) | O que é proibido |

## Visão de produto

A plataforma **não vende módulos isolados**. Vende **soluções operacionais** (Eletroposto, Estacionamento, Concessionária, etc.). Módulos são capacidades internas; o cliente enxerga o pacote adequado ao negócio.

## Estrutura do repositório

```
docs/architecture/     Constituição e domínios
src/                   Solução .NET (modular monolith)
  BuildingBlocks/      Contratos, eventos, audit trail
  Core/                Empresa, usuário, auth, licença
  Cadastros/           Cliente, veículo, equipamento…
  Operacao/            Atendimento + especializações
  Financeiro/          Caixa, recebimento
  Host.Api/            API unificada
  Web/                 Frontend (SPA)
tests/                 Testes unitários e de integração
```

## Princípio operacional #1

**Todo serviço prestado gera um Atendimento.**  
Carregamento, estacionamento, lavagem, oficina e valet são especializações — não sistemas paralelos.

## Status

| Fase | Estado |
|------|--------|
| 0 — Constituição + Domain Model | Em andamento |
| 1 — Core + Cadastros + Atendimento + Carregamento | Planejada |
| 2+ | Ver [Roadmap](docs/architecture/05-ROADMAP.md) |

## Autoria

Anderson Pereira Silva — 29/07/2026
