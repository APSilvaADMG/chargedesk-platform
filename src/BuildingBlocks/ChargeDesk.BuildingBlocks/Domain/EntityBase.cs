// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Contrato base de entidades da plataforma (multiempresa + soft delete).

namespace ChargeDesk.BuildingBlocks.Domain;

public abstract class EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Ativo;
    public DateTime CriadoEm { get; set; }
    public Guid? CriadoPor { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    public Guid? AtualizadoPor { get; set; }
    public DateTime? ExcluidoEm { get; set; }
    public Guid? ExcluidoPor { get; set; }
    public int Versao { get; set; } = 1;
}

public enum EntityStatus : byte
{
    Ativo = 1,
    Inativo = 2,
    Bloqueado = 3,
    Cancelado = 4,
    Arquivado = 5
}
