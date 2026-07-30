// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: Cobrança por tempo — carregamento e estacionamento (faixas).

namespace ChargeDesk.Operacao.Application;

public record TarifaConfig(decimal PrimeiraHora, decimal HoraExcedente, int MinutosPrimeiraHora = 60);

public readonly record struct DetalheCobranca(
    int MinutosTotais,
    int MinutosExcedentes,
    decimal ValorPrimeiraFaixa,
    decimal ValorExcedente,
    decimal Total);

public static class CobrancaService
{
    public static readonly TarifaConfig Padrao = new(20m, 50m, 60);
    public static readonly TarifaConfig EstacionamentoPadrao = new(10m, 5m, 60);

    public static int CalcularMinutos(DateTime inicio, DateTime fim)
    {
        if (fim < inicio) fim = fim.AddDays(1);
        return (int)Math.Round((fim - inicio).TotalMinutes);
    }

    public static decimal CalcularValor(int minutos, TarifaConfig? tarifa = null)
        => Detalhar(minutos, tarifa).Total;

    public static DetalheCobranca Detalhar(int minutos, TarifaConfig? tarifa = null)
    {
        tarifa ??= Padrao;
        if (minutos <= 0)
            return new DetalheCobranca(0, 0, 0m, 0m, 0m);

        var limite = tarifa.MinutosPrimeiraHora <= 0 ? 60 : tarifa.MinutosPrimeiraHora;
        var primeira = tarifa.PrimeiraHora;
        var excedenteMin = Math.Max(minutos - limite, 0);
        var total = excedenteMin == 0
            ? primeira
            : Math.Round(primeira + excedenteMin * (tarifa.HoraExcedente / 60m), 2, MidpointRounding.AwayFromZero);
        var valorExcedente = Math.Round(total - primeira, 2, MidpointRounding.AwayFromZero);
        return new DetalheCobranca(minutos, excedenteMin, primeira, valorExcedente, total);
    }

    public static string FormatarTempo(int minutos)
    {
        var h = minutos / 60;
        var m = minutos % 60;
        return $"{h:00}:{m:D2}";
    }
}
