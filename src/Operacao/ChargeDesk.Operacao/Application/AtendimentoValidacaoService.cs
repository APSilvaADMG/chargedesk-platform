// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Validações de atendimento (carregamento e estacionamento).

using ChargeDesk.Operacao.Domain;

namespace ChargeDesk.Operacao.Application;

public static class AtendimentoValidacaoService
{
    public const string MensagemEquipamentoIndisponivel =
        "Este ponto de carregamento não está mais disponível. Selecione outro ponto.";

    public const string MensagemVagaIndisponivel =
        "Esta vaga não está mais disponível. Selecione outra vaga.";

    public static string? ValidarTicket(int ticket, bool duplicado)
    {
        if (ticket <= 0) return "Informe o número do ticket do bloco.";
        if (duplicado) return $"Ticket #{ticket} já está cadastrado.";
        return null;
    }

    public static string? ValidarEquipamento(bool ativo, bool ocupado)
    {
        if (!ativo) return "Ponto de carga não encontrado ou inativo.";
        if (ocupado) return MensagemEquipamentoIndisponivel;
        return null;
    }

    public static string? ValidarVaga(bool ativo, bool ocupado)
    {
        if (!ativo) return "Vaga não encontrada ou inativa.";
        if (ocupado) return MensagemVagaIndisponivel;
        return null;
    }

    public static string? ValidarCaixaAbertoParaIniciar(bool caixaAberto)
    {
        if (!caixaAberto)
            return "Abra o caixa antes de iniciar um novo atendimento.";
        return null;
    }

    public static string? ValidarFechamentoSemAtendimentosAtivos(int emExecucao)
    {
        if (emExecucao <= 0) return null;
        if (emExecucao == 1)
            return "Não é possível fechar o caixa com 1 atendimento em andamento. Finalize ou cancele o atendimento ativo.";
        return $"Não é possível fechar o caixa com {emExecucao} atendimentos em andamento. Finalize ou cancele os atendimentos ativos.";
    }

    public static string? ValidarHorarios(DateTime inicio, DateTime? fim)
    {
        if (fim.HasValue && fim <= inicio)
            return "Hora final deve ser posterior à hora inicial.";
        return null;
    }

    public static bool EquipamentoDisponivel(bool ativo, bool possuiAtendimentoEmExecucao)
        => ativo && !possuiAtendimentoEmExecucao;

    public static string? ValidarTransicao(AtendimentoStatus de, AtendimentoStatus para)
        => AtendimentoStateMachine.Validar(de, para);
}
