# 04 — Modelo Físico (Fase 1 — MVP)

<!-- Autor: Anderson Pereira Silva -->
<!-- Data: 29/07/2026 -->
<!-- Descrição: Tabelas mínimas para Core + Cadastros + Atendimento Carregamento + Caixa. -->

## Escopo Fase 1

Entregar paridade operacional com o ChargeDesk atual:

- Login / usuários / perfis básicos  
- Clientes e veículos (cliente herdado do veículo no atendimento)  
- Equipamentos tipo Carregador (ex-pontos)  
- Atendimento tipo Carregamento  
- Caixa + recebimentos  
- Auditoria básica  
- Persistência durable (não efêmera de container)  

Fora da Fase 1: Estacionamento, OS, CRM, Contas a pagar, Capabilities, Campos dinâmicos.

---

## Tabelas Core

| Tabela | Notas |
|--------|-------|
| Empresa | Tenant |
| Unidade | Local físico / operação |
| Usuario | Login, hash, ativo |
| Perfil | Admin, Operador, … |
| Permissao | Catálogo |
| PerfilPermissao | N:N |
| UsuarioPerfil | N:N (ou PerfilId direto no MVP) |
| EmpresaLicenca | Módulos habilitados |
| Configuracao | Chave/valor por empresa |
| Auditoria | Ação, entidade, usuário, IP |
| BackupHistorico | Metadados de backup |

---

## Tabelas Cadastros

| Tabela | Notas |
|--------|-------|
| Cliente | Nome; telefone pode ir em Contato desde o início |
| ClienteContato | Telefone, e-mail, tipo |
| ClienteEndereco | Opcional no MVP UI; schema já previsto |
| Veiculo | ClienteId, placa, marca, modelo, ano, cor, conector, obs |
| Equipamento | Tipo=Carregador no MVP; Nome, UnidadeId, Ativo |
| FormaPagamento | Pix, Dinheiro, Débito, Crédito, Cortesia |
| TabelaPreco | Tarifa carregamento (faixas) |

---

## Tabelas Operação

| Tabela | Notas |
|--------|-------|
| Atendimento | Núcleo (substitui SessaoCarregamento) |
| AtendimentoTipo | Seed: Carregamento=1 |
| AtendimentoCarregamento | Extensão: EquipamentoId (ponto), kWh?, potência?, ticket |
| HistoricoOperacao | Timeline do atendimento |

### Atendimento (campos essenciais)

`Id, EmpresaId, UnidadeId, ClienteId, VeiculoId, TipoId, Status, Origem, Ticket, AbertoEm, EncerradoEm, OperadorId, Observacoes, Versao, auditoria…`

### AtendimentoCarregamento

`AtendimentoId (PK/FK), EquipamentoId, EnergiaKwh, PotenciaKw, TempoMinutos, ValorCalculado, …`

---

## Tabelas Financeiro (MVP)

| Tabela | Notas |
|--------|-------|
| Caixa | Abertura/fechamento por unidade/estação |
| CaixaMovimento | Sangria, suprimento, recebimento |
| Recebimento | AtendimentoId, forma, valor, caixaId |

---

## Convenções físicas

- PK: `Id`  
- FK: `ClienteId`, `EmpresaId`, …  
- Soft delete + `Versao`  
- Índice em **toda** FK e em (`EmpresaId`, `Status`), (`EmpresaId`, `Placa`), (`Ticket`) único por empresa  
- Nomes sem abreviação: `Cliente`, não `TB_CLI`  

---

## Mapeamento legado → Fase 1

| ChargeDesk atual | Platform |
|------------------|----------|
| SessaoCarregamento | Atendimento + AtendimentoCarregamento |
| PontoCarregamento | Equipamento (Tipo=Carregador) |
| Cliente.Telefone | ClienteContato (Tipo=Telefone) ou coluna ponte no MVP |
| Caixa / Movimentacao | Caixa / CaixaMovimento |

Estratégia de migração de dados: script ETL na Fase 1.1 (após paridade funcional em ambiente limpo).
