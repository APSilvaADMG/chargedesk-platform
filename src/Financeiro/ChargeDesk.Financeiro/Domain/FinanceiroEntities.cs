// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Entidades de Caixa (MVP Financeiro).

using ChargeDesk.BuildingBlocks.Domain;

namespace ChargeDesk.Financeiro.Domain;

public class Caixa : EntityBase
{
    public Guid UnidadeId { get; set; }
    public Guid OperadorId { get; set; }
    public int Numero { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public decimal ValorInicial { get; set; }
    public decimal? ValorInformado { get; set; }
    public string? ObservacoesFechamento { get; set; }
    public StatusCaixa StatusCaixa { get; set; } = StatusCaixa.Aberto;
}

public enum StatusCaixa : byte
{
    Aberto = 1,
    Fechado = 2
}

public class Recebimento : EntityBase
{
    public Guid CaixaId { get; set; }
    public Guid AtendimentoId { get; set; }
    public FormaPagamento Forma { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataHora { get; set; }
}

public enum FormaPagamento : byte
{
    Pix = 1,
    Dinheiro = 2,
    Debito = 3,
    Credito = 4,
    Cortesia = 5
}
