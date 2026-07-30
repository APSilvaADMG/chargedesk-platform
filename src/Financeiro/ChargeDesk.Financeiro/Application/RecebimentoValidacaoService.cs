// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Validações de recebimento/pagamento (dono: Financeiro).

using ChargeDesk.Financeiro.Domain;

namespace ChargeDesk.Financeiro.Application;

public static class RecebimentoValidacaoService
{
    public static string? ValidarCortesia(FormaPagamento forma, string? motivo)
    {
        if (forma == FormaPagamento.Cortesia && string.IsNullOrWhiteSpace(motivo))
            return "Informe o motivo da cortesia.";
        return null;
    }

    public static decimal ValorRecebimentoPagamento(FormaPagamento forma, decimal valorSessao)
        => forma == FormaPagamento.Cortesia ? 0m : valorSessao;
}
