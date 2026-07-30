# 02 — Filosofia de Desenvolvimento

<!-- Autor: Anderson Pereira Silva -->
<!-- Data: 29/07/2026 -->
<!-- Descrição: Cinco pilares e princípios de produto/engenharia. -->

## Objetivo

Construir um ecossistema sólido por muitos anos.  
Cada decisão técnica considera impacto de médio e longo prazo — não apenas a entrega imediata.

---

## Cinco pilares

### 1. Simplicidade
A solução mais simples que resolve corretamente. Complexidade só quando necessária.

### 2. Consistência
O mesmo problema, a mesma forma — em todos os módulos.

### 3. Escalabilidade
Projetar para: multiempresa, multiunidade, muitos usuários, novos módulos, equipamentos e integrações.

### 4. Reutilização
Impressão, auditoria, auth, notificação, pagamento, upload, validação e cálculo — centralizados.

### 5. Evolução contínua
Novos módulos **sem reescrever** os existentes.

---

## Checklist obrigatório antes de implementar

| Pergunta | Resposta exigida |
|----------|------------------|
| Resolve o problema? | Sim |
| Não quebra o que já funciona? | Sim |
| É reutilizável? | Sim (ou justificado) |
| É simples? | Sim |
| É fácil de manter? | Sim |
| Serve a qualquer módulo futuro? | Sim (ou extensão clara) |
| Serve a qualquer empresa/unidade? | Sim |

Se alguma resposta for **não**, reavaliar.

---

## Single Source of Truth

| Dado | Dono |
|------|------|
| Cliente / Veículo / Equipamento | Cadastros |
| Empresa / Usuário / Permissão | Core |
| Atendimento | Operação |
| Pagamento / Caixa | Financeiro |

---

## Responsabilidade única

- Core autentica e autoriza  
- Cadastros guardam mestres  
- Operação executa atendimentos  
- Financeiro controla dinheiro  
- Relatórios apenas apresentam (via Services)  

---

## Expansão natural

Nova necessidade → pertence a módulo existente **ou** nasce módulo novo.  
Nunca forçar Estacionamento a “virar” Oficina.

---

## Evolução sem regressão

Antes de alterar: impacto técnico, funcional, migração, desempenho, segurança.  
Nenhuma alteração autoriza quebrar fluxo homologado.

---

## Configuração acima do código

Tolerância, tabela de preço, mensagens, impressão, permissões, licenciamento e fluxos: preferir configuração.

---

## Observabilidade

Ação importante → Log + Evento + Auditoria + métrica (+ notificação quando fizer sentido).

---

## Experiência do operador

Poucos cliques, pouca digitação, automação do repetitivo, feedback imediato.

## Experiência do gestor

Responder rápido: faturamento do dia, ocupação, equipamentos offline, ticket médio, serviços mais vendidos.

---

## Integração

ERP, gateway, equipamentos físicos, WhatsApp/e-mail/SMS, apps e marketplaces devem ser possíveis sem redesign do Core.

---

## LGPD e retenção

Soft delete é o padrão operacional. Além disso:

- direito de acesso / exportação
- anonimização sob solicitação legal
- política de retenção configurável por empresa

“Nunca apagar” **não** significa ignorar obrigações legais.

---

## Produto

A ChargeDesk Platform vende **soluções** (bundles), não catálogo solto de módulos.
