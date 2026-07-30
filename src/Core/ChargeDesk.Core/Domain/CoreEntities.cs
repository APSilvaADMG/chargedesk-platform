// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Entidades do domínio Core (multiempresa).

using ChargeDesk.BuildingBlocks.Domain;

namespace ChargeDesk.Core.Domain;

public class Empresa : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public string? Documento { get; set; }
}

public class Unidade : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public string? Codigo { get; set; }
}

public class Usuario : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public Guid? UnidadeId { get; set; }
    public bool Admin { get; set; }
    public DateTime? UltimoAcesso { get; set; }
}

public class EmpresaLicenca : EntityBase
{
    public string Modulo { get; set; } = string.Empty;
    public DateTime? ValidoAte { get; set; }
}

public class AuditoriaRegistro : EntityBase
{
    public Guid? UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty;
    public string? Entidade { get; set; }
    public Guid? EntidadeId { get; set; }
    public string? Detalhe { get; set; }
    public string? Ip { get; set; }
}
