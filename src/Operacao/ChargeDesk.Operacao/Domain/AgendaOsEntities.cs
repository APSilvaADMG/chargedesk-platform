// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Entidades Agenda e Ordem de Serviço (Fase 3 MVP).

using ChargeDesk.BuildingBlocks.Domain;

namespace ChargeDesk.Operacao.Domain;

public class AgendaReserva : EntityBase
{
    public Guid UnidadeId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid? VeiculoId { get; set; }
    public AtendimentoTipo TipoServico { get; set; } = AtendimentoTipo.Oficina;
    public DateTime InicioPrevisto { get; set; }
    public DateTime? FimPrevisto { get; set; }
    public AgendaStatus StatusAgenda { get; set; } = AgendaStatus.Agendada;
    public Guid? AtendimentoId { get; set; }
    public string? Observacoes { get; set; }
}

public enum AgendaStatus : short
{
    Agendada = 1,
    Confirmada = 2,
    EmAtendimento = 3,
    Concluida = 4,
    Cancelada = 5,
    NoShow = 6
}

public class OrdemServico : EntityBase
{
    public Guid UnidadeId { get; set; }
    public Guid AtendimentoId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid? VeiculoId { get; set; }
    public int Numero { get; set; }
    public OrdemServicoStatus StatusOs { get; set; } = OrdemServicoStatus.Aberta;
    public string? Diagnostico { get; set; }
    public string? Observacoes { get; set; }
    public DateTime AbertaEm { get; set; }
    public DateTime? EncerradaEm { get; set; }
}

public enum OrdemServicoStatus : short
{
    Aberta = 1,
    EmExecucao = 2,
    AguardandoPecas = 3,
    AguardandoAprovacao = 4,
    Concluida = 5,
    Cancelada = 6
}

public class OrdemServicoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrdemServicoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public OrdemServicoItemTipo Tipo { get; set; } = OrdemServicoItemTipo.Servico;
    public decimal Quantidade { get; set; } = 1;
    public decimal ValorUnitario { get; set; }
    public bool Concluido { get; set; }
}

public enum OrdemServicoItemTipo : short
{
    Servico = 1,
    Peca = 2,
    Checklist = 3,
    Observacao = 4
}
