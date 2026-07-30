// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Extensão de Atendimento para estacionamento.

namespace ChargeDesk.Operacao.Domain;

public class AtendimentoEstacionamento
{
    public Guid AtendimentoId { get; set; }
    public Guid? EquipamentoId { get; set; }
    public string? Vaga { get; set; }
    public int? TempoMinutos { get; set; }
    public decimal? ValorCalculado { get; set; }
}
