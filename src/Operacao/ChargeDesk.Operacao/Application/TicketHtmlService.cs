// Autor: Anderson Pereira Silva
// Data: 30/07/2026
// Descrição: HTML simples de ticket (impressão navegador) — paridade operacional.

using System.Globalization;
using System.Net;
using System.Text;

namespace ChargeDesk.Operacao.Application;

public static class TicketHtmlService
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string GerarInicio(
        int ticket,
        string titulo,
        string cliente,
        string placa,
        string recurso,
        DateTime entrada,
        TarifaConfig? tarifa = null)
    {
        tarifa ??= CobrancaService.Padrao;
        var ticketFmt = ticket.ToString("D6");
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Ticket ")
          .Append(ticketFmt).Append("</title><style>");
        sb.Append("body{font-family:Arial,sans-serif;width:280px;margin:12px auto;font-size:13px}");
        sb.Append("h1{font-size:16px;margin:0 0 8px;text-align:center}");
        sb.Append(".box{border:1px solid #333;padding:8px;margin:8px 0}");
        sb.Append(".row{display:flex;justify-content:space-between;margin:4px 0}");
        sb.Append(".attn{background:#fff3cd;border:1px solid #ffc107;padding:8px;margin-top:8px}");
        sb.Append(".barcode{text-align:center;font-size:22px;letter-spacing:3px;margin-top:10px;font-family:monospace}");
        sb.Append("@media print{button{display:none}}");
        sb.Append("</style></head><body>");
        sb.Append("<h1>").Append(WebUtility.HtmlEncode(titulo)).Append("</h1>");
        sb.Append("<div class=\"box\">");
        Linha(sb, "Ticket", "#" + ticketFmt);
        Linha(sb, "Cliente", cliente);
        Linha(sb, "Placa", placa);
        Linha(sb, "Recurso", recurso);
        Linha(sb, "Entrada", entrada.ToString("dd/MM/yyyy HH:mm", PtBr));
        sb.Append("</div>");
        sb.Append("<div class=\"attn\"><strong>Tarifa</strong><br>");
        sb.Append("Até 60 min: ").Append(Moeda(tarifa.PrimeiraHora)).Append(" (fixo)<br>");
        sb.Append("Após 60 min: ").Append(Moeda(tarifa.HoraExcedente)).Append("/h</div>");
        sb.Append("<div class=\"barcode\">*").Append(ticketFmt).Append("*</div>");
        sb.Append("<p style=\"text-align:center\"><button onclick=\"window.print()\">Imprimir</button></p>");
        sb.Append("</body></html>");
        return sb.ToString();
    }

    private static void Linha(StringBuilder sb, string k, string v)
        => sb.Append("<div class=\"row\"><span>").Append(WebUtility.HtmlEncode(k))
             .Append("</span><strong>").Append(WebUtility.HtmlEncode(v ?? "—")).Append("</strong></div>");

    private static string Moeda(decimal v) => v.ToString("C", PtBr);
}
