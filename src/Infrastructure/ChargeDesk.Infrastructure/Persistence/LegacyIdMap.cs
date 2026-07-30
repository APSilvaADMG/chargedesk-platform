// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Mapa int legado → Guid Platform (importação SQLite ChargeDesk).

namespace ChargeDesk.Infrastructure.Persistence;

public class LegacyIdMap
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Tabela { get; set; } = string.Empty;
    public int LegacyId { get; set; }
    public Guid NovoId { get; set; }
}
