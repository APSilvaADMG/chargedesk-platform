// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Horário operacional America/Sao_Paulo (herança ChargeDesk).

namespace ChargeDesk.BuildingBlocks.Time;

public static class HorarioOperacional
{
    private static readonly TimeZoneInfo Fuso = ResolverFuso();

    private static TimeZoneInfo ResolverFuso()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows()
                    ? "E. South America Standard Time"
                    : "America/Sao_Paulo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.CreateCustomTimeZone(
                "BRT", TimeSpan.FromHours(-3), "Horário de Brasília", "BRT");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.CreateCustomTimeZone(
                "BRT", TimeSpan.FromHours(-3), "Horário de Brasília", "BRT");
        }
    }

    public static DateTime Agora()
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Fuso);
}
