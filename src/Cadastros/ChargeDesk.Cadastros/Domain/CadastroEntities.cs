// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Entidades mestres de Cadastros.

using ChargeDesk.BuildingBlocks.Domain;

namespace ChargeDesk.Cadastros.Domain;

public class Cliente : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Observacoes { get; set; }
}

public class Veiculo : EntityBase
{
    public Guid ClienteId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public int? Ano { get; set; }
    public string? Cor { get; set; }
    public string? Conector { get; set; }
    public string? Observacoes { get; set; }
}

public class Equipamento : EntityBase
{
    public Guid UnidadeId { get; set; }
    public EquipamentoTipo Tipo { get; set; } = EquipamentoTipo.Carregador;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}

public enum EquipamentoTipo : short
{
    Carregador = 1,
    Cancela = 2,
    Impressora = 3,
    Totem = 4,
    Sensor = 5,
    Camera = 6,
    Ocr = 7,
    Rfid = 8,
    Display = 9,
    Outro = 99
}
