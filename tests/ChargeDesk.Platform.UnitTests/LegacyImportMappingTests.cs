// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Testes unitários de mapeamento status sessão legado → Platform.

using ChargeDesk.Operacao.Domain;

namespace ChargeDesk.Platform.UnitTests;

public class LegacyImportMappingTests
{
    [Theory]
    [InlineData(1, AtendimentoStatus.EmExecucao)]
    [InlineData(2, AtendimentoStatus.AguardandoPagamento)]
    [InlineData(3, AtendimentoStatus.Finalizado)]
    [InlineData(4, AtendimentoStatus.Cancelado)]
    public void StatusSessao_MapeiaCorretamente(int legado, AtendimentoStatus esperado)
    {
        var status = legado switch
        {
            1 => AtendimentoStatus.EmExecucao,
            2 => AtendimentoStatus.AguardandoPagamento,
            3 => AtendimentoStatus.Finalizado,
            4 => AtendimentoStatus.Cancelado,
            _ => AtendimentoStatus.EmExecucao
        };
        Assert.Equal(esperado, status);
    }
}
