// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Extensão de Atendimento para carregamento elétrico.

namespace ChargeDesk.Operacao.Domain;

public class AtendimentoCarregamento
{
    public Guid AtendimentoId { get; set; }
    public Guid EquipamentoId { get; set; }
    public decimal? EnergiaKwh { get; set; }
    public decimal? PotenciaKw { get; set; }
    public int? TempoMinutos { get; set; }
    public decimal? ValorCalculado { get; set; }
}
