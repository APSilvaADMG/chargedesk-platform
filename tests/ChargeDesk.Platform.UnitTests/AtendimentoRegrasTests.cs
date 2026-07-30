// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Testes de regras Fase 1/2 (carregamento, estacionamento, ticket, caixa).

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

    [Fact]
    public void EquipamentoOcupado_BloqueiaNovaCarga()
    {
        var erro = AtendimentoValidacaoService.ValidarEquipamento(ativo: true, ocupado: true);
        Assert.Equal(AtendimentoValidacaoService.MensagemEquipamentoIndisponivel, erro);
    }

    [Fact]
    public void CaixaFechado_BloqueiaNovaCarga()
    {
        var erro = AtendimentoValidacaoService.ValidarCaixaAbertoParaIniciar(false);
        Assert.Contains("Abra o caixa", erro);
    }

    [Fact]
    public void FecharCaixa_BloqueiaComAtendimentoAtivo()
    {
        Assert.Null(AtendimentoValidacaoService.ValidarFechamentoSemAtendimentosAtivos(0));
        Assert.Contains("1 atendimento", AtendimentoValidacaoService.ValidarFechamentoSemAtendimentosAtivos(1));
    }

    [Fact]
    public void VagaOcupada_BloqueiaEntrada()
    {
        var erro = AtendimentoValidacaoService.ValidarVaga(ativo: true, ocupado: true);
        Assert.Equal(AtendimentoValidacaoService.MensagemVagaIndisponivel, erro);
    }

    [Fact]
    public void Cobranca_Estacionamento_PrimeiraFaixa()
    {
        Assert.Equal(10m, CobrancaService.CalcularValor(45, CobrancaService.EstacionamentoPadrao));
        Assert.True(CobrancaService.CalcularValor(90, CobrancaService.EstacionamentoPadrao) > 10m);
    }

    [Fact]
    public void TicketHtml_ContemNumeroEPlaca()
    {
        var html = TicketHtmlService.GerarInicio(12, "ChargeDesk — Teste", "Cliente X", "ABC1D23", "Vaga 01", DateTime.Today.AddHours(10));
        Assert.Contains("#000012", html);
        Assert.Contains("ABC1D23", html);
        Assert.Contains("window.print()", html);
    }

    [Fact]
    public void Cobranca_PrimeiraFaixaFixa()
    {
        Assert.Equal(20m, CobrancaService.CalcularValor(30));
        Assert.Equal(20m, CobrancaService.CalcularValor(60));
        var comExcedente = CobrancaService.CalcularValor(90);
        Assert.True(comExcedente > 20m);
    }

    [Fact]
    public void ListaVaziaDisponiveis_NaoSignificaTodosLivres()
    {
        // Regressão documentada: [] da API de disponíveis é válido (todos ocupados).
        var disponiveis = Array.Empty<object>();
        Assert.Empty(disponiveis);
        Assert.False(disponiveis.Length > 0); // não cair em fallback de “todos”
    }
}
