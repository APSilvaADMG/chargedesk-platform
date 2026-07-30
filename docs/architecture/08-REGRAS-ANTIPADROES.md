# 08 — Anti-padrões (proibições)

<!-- Autor: Anderson Pereira Silva -->
<!-- Data: 29/07/2026 -->
<!-- Descrição: Lista explícita do que a plataforma não permite. -->

## Arquitetura

| Proibido | Motivo |
|----------|--------|
| Módulo A chama serviço interno do módulo B | Acoplamento |
| SQL/join em tabela de outro domínio | Viola Ownership |
| Microserviços no dia 1 “porque é moderno” | Complexidade prematura |
| God service / God controller | Manutenção impossível |
| Copiar entidade Cliente dentro de cada módulo | Duplicação |

## Dados

| Proibido | Motivo |
|----------|--------|
| Hard delete de atendimento/pagamento em produção | Auditoria |
| `CodigoCliente` / `CodCli` / `TB_CLI` | Fora do padrão |
| Status como string livre `"OK"` | Fragilidade |
| ClienteId opcional em Veículo | Integridade |
| Dois clientes diferentes no mesmo atendimento vs. dono do veículo | Divergência |

## Frontend

| Proibido | Motivo |
|----------|--------|
| Calcular tarifa só no JS sem validar na API | Fraude / divergência |
| Fallback que lista todos os equipamentos quando API retorna `[]` | Regressão conhecida |
| Regra de permissão só escondendo botão | Segurança |

## Operação

| Proibido | Motivo |
|----------|--------|
| Dois atendimentos EmExecucao no mesmo carregador | Contenção física |
| Abrir atendimento sem caixa (quando módulo Caixa licenciado) | Controle |
| Alterar máquina de estados livremente | Integridade |

## Processo

| Proibido | Motivo |
|----------|--------|
| Feature nova sem atualizar Domain Model (doc 03) | Constituição |
| “Aproveitar e refatorar” módulo não pedido | Escopo / regressão |
| Mentão a ferramentas de IA em commits/código | Padrão do autor |

## Infra

| Proibido | Motivo |
|----------|--------|
| Banco só no filesystem efêmero do container | Perda de dados |
| Deploy sem tag de imagem única | Revisões fantasma |
| Host Azure sem fuso Brasília para horários operacionais | Divergência de hora |
