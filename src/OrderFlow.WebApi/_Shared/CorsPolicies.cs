namespace OrderFlow.WebApi._Shared;

public static class CorsPolicies
{
    public const string Frontend = "Frontend";

    public const string AllowedOriginsSection = "Cors:AllowedOrigins";

    /// <summary>
    /// O dev server do frontend responde nos dois hosts; o navegador trata cada um como
    /// uma origem distinta, então ambos precisam estar liberados.
    /// </summary>
    public static readonly string[] DefaultAllowedOrigins =
    [
        "http://localhost:5173",
        "http://127.0.0.1:5173"
    ];

    public static readonly TimeSpan PreflightMaxAge = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Origem terminada em barra nunca casa com a do navegador — e configurada por variável de ambiente
    /// o erro passa despercebido, aparecendo só como CORS bloqueado no browser.
    /// </summary>
    public static string[] Normalize(string[] origins)
    {
        return [.. origins.Select(origin => origin.Trim().TrimEnd('/'))];
    }
}
