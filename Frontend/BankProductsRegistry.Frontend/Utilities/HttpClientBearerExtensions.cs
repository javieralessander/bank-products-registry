using System.Net.Http.Headers;

namespace BankProductsRegistry.Frontend.Utilities;

/// <summary>
/// Evita cabeceras Authorization duplicadas o mezcladas al reutilizar el HttpClient con nombre.
/// </summary>
public static class HttpClientBearerExtensions
{
    public static void SetBearerToken(this HttpClient client, string? bearerToken)
    {
        client.DefaultRequestHeaders.Remove("Authorization");
        var trimmed = bearerToken?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", trimmed);
    }
}
