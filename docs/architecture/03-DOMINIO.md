# 03 — Modelo de Domínio

<!-- Autor: Anderson Pereira Silva -->
<!-- Data: 29/07/2026 -->
<!-- Descrição: Entidades oficiais e relacionamento Atendimento-centrado. -->

## Hierarquia

```
ChargeDesk Platform
  → Domínios
    → Entidades
      → Serviços
```

Nenhuma entidade oficial existe fora deste documento.  
Nova entidade = atualizar este arquivo **antes** do código.

---

## Domínio Core

Empresa, Unidade, Usuário, Perfil, Permissão, Licença, Configuração, Auditoria, Log, Notificação, Backup, Arquivo.

**Responsabilidade:** infraestrutura compartilhada. Não conhece regra de Carregamento/Estacionamento.

---

## Domínio Cadastros

Cliente, Veículo, Equipamento, Produto, Serviço, Convênio, Categoria, Endereço, Documento, Contato, TabelaPreco, FormaPagamento.

### Cliente
Entidade central de relacionamento. Possui veículos, atendimentos, cobranças, documentos, histórico.

### Veículo
Pertence obrigatoriamente a um Cliente (`ClienteId`). Usado por todos os módulos.

### Equipamento
Qualquer recurso físico: carregador, cancela, impressora, totem, sensor, câmera, OCR, RFID, display.

---

## Domínio Operação

**Atendimento** (centro), Agenda, Reserva, OrdemServico, Checklist, Execução, HistóricoOperacional.

### Atendimento — entidade mais importante

Todo serviço prestado gera um Atendimento.

Especializações (extensões, não sistemas paralelos):

| Tipo | Extensão típica |
|------|-----------------|
| Carregamento | energia, conector, potência, ponto |
| Estacionamento | vaga, entrada/saída, permanência |
| Lavagem | pacote, boxes |
| Oficina | OS, peças, checklist |
| Valet | motorista, chave |

### Ciclo de vida (máquina de estados)

```
Criado → Agendado → Confirmado → CheckIn → EmExecucao
  → AguardandoPagamento → Finalizado → Arquivado
```

(Cancelado possível a partir de estados permitidos.)

Transições **controladas** — proibido `Finalizado → EmExecucao`.

Nem todo tipo usa todas as etapas; todos usam o mesmo conceito.

### Conteúdo mínimo do Atendimento

Empresa, Unidade, Cliente, Veículo (quando aplicável), Tipo, Status, Origem, abertura/encerramento, responsável/operador, equipamentos, produtos, serviços, observações, documentos, fotos, histórico, auditoria.

---

## Domínio Financeiro

Caixa, CaixaMovimento, Recebimento, Pagamento, ContaReceber, ContaPagar, Fatura, Mensalidade, Conciliação, Estorno.

**Contrato:**

- Caixa = turno do operador  
- Recebimento = liquidação de atendimento / movimento de caixa  
- Contas a receber / fatura / mensalidade = crédito e recorrência  

---

## Domínio Relatórios

Sem tabelas de negócio próprias na Fase 1. Consome Services dos outros domínios.

---

## Domínio Integrações / Comunicação

Webhook, ApiExterna, Fila, Mensagem (Email/SMS/WhatsApp/Push). Fase posterior — contrato previsto.

---

## Relacionamentos-mestre

```
Empresa
  ├── Unidades
  ├── Usuários
  ├── Clientes
  │     └── Veículos
  ├── Equipamentos
  ├── Atendimentos
  │     ├── Produtos / Serviços / Equipamentos
  │     └── Recebimentos
  └── Caixas
```

---

## Capacidades (visão Fase 2+)

Capabilities são composições reutilizáveis (ReceberPagamento, EmitirTicket, ControlarPermanencia, ConsumirEnergia…).  
**Fase 1** usa Atendimento tipado + Services. Capabilities entram quando houver 2+ especializações estáveis.

---

## Campos personalizados (Fase 2)

`CampoPersonalizado` + `ValorCampoPersonalizado` por Empresa/Entidade — sem alterar schema a cada cliente.
