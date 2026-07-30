// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Testes da máquina de estados e herança cliente←veículo.

using ChargeDesk.Operacao.Application;
using ChargeDesk.Operacao.Domain;

namespace ChargeDesk.Platform.UnitTests;

public class AtendimentoRegrasTests
{
    [Fact]
    public void Cliente_SempreDoVeiculo()
    {
        var dono = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var requestErrada = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var resolvido = AtendimentoClienteResolver.ResolverClienteId(dono);
        Assert.Equal(dono, resolvido);
        Assert.NotEqual(requestErrada, resolvido);
    }

    [Fact]
    public void Transicao_FinalizadoParaEmExecucao_Bloqueada()
    {
        var erro = AtendimentoStateMachine.Validar(
            AtendimentoStatus.Finalizado, AtendimentoStatus.EmExecucao);
        Assert.NotNull(erro);
    }

    [Fact]
    public void Transicao_CriadoParaEmExecucao_Permitida()
    {
        Assert.True(AtendimentoStateMachine.PodeTransicionar(
            AtendimentoStatus.Criado, AtendimentoStatus.EmExecucao));
    }
}
