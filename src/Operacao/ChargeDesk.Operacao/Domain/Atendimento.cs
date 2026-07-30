// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Atendimento — entidade central da operação.

using ChargeDesk.BuildingBlocks.Domain;

namespace ChargeDesk.Operacao.Domain;

public class Atendimento : EntityBase
{
    public Guid UnidadeId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid? VeiculoId { get; set; }
    public AtendimentoTipo Tipo { get; set; }
    public AtendimentoStatus StatusAtendimento { get; set; } = AtendimentoStatus.Criado;
    public AtendimentoOrigem Origem { get; set; } = AtendimentoOrigem.Manual;
    public int? Ticket { get; set; }
    public DateTime AbertoEm { get; set; }
    public DateTime? EncerradoEm { get; set; }
    public Guid? OperadorId { get; set; }
    public string? Observacoes { get; set; }
}

public enum AtendimentoTipo : short
{
    Carregamento = 1,
    Estacionamento = 2,
    Lavagem = 3,
    Oficina = 4,
    Valet = 5,
    Outro = 99
}

public enum AtendimentoStatus : short
{
    Criado = 1,
    Agendado = 2,
    Confirmado = 3,
    CheckIn = 4,
    EmExecucao = 5,
    AguardandoPagamento = 6,
    Finalizado = 7,
    Cancelado = 8,
    Arquivado = 9
}

public enum AtendimentoOrigem : short
{
    Manual = 1,
    Agenda = 2,
    Totem = 3,
    Integracao = 4
}
