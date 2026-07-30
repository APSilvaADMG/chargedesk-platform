// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Máquina de estados do Atendimento (transições permitidas).

namespace ChargeDesk.Operacao.Domain;

public static class AtendimentoStateMachine
{
    private static readonly Dictionary<AtendimentoStatus, HashSet<AtendimentoStatus>> Permitidas = new()
    {
        [AtendimentoStatus.Criado] = [AtendimentoStatus.Agendado, AtendimentoStatus.CheckIn, AtendimentoStatus.EmExecucao, AtendimentoStatus.Cancelado],
        [AtendimentoStatus.Agendado] = [AtendimentoStatus.Confirmado, AtendimentoStatus.CheckIn, AtendimentoStatus.Cancelado],
        [AtendimentoStatus.Confirmado] = [AtendimentoStatus.CheckIn, AtendimentoStatus.Cancelado],
        [AtendimentoStatus.CheckIn] = [AtendimentoStatus.EmExecucao, AtendimentoStatus.Cancelado],
        [AtendimentoStatus.EmExecucao] = [AtendimentoStatus.AguardandoPagamento, AtendimentoStatus.Finalizado, AtendimentoStatus.Cancelado],
        [AtendimentoStatus.AguardandoPagamento] = [AtendimentoStatus.Finalizado, AtendimentoStatus.Cancelado],
        [AtendimentoStatus.Finalizado] = [AtendimentoStatus.Arquivado],
        [AtendimentoStatus.Cancelado] = [AtendimentoStatus.Arquivado],
        [AtendimentoStatus.Arquivado] = []
    };

    public static bool PodeTransicionar(AtendimentoStatus de, AtendimentoStatus para)
        => Permitidas.TryGetValue(de, out var destinos) && destinos.Contains(para);

    public static string? Validar(AtendimentoStatus de, AtendimentoStatus para)
        => PodeTransicionar(de, para)
            ? null
            : $"Transição inválida: {de} → {para}.";
}
