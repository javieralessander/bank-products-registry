using System.Text.Json;

namespace BankProductsRegistry.Frontend.Utilities;

public static class ApiErrorParser
{
    public static async Task<string?> ExtractMessageAsync(HttpResponseMessage response)
    {
        var raw = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("detail", out var detailElement) &&
                    detailElement.ValueKind == JsonValueKind.String)
                {
                    return detailElement.GetString();
                }

                if (root.TryGetProperty("title", out var titleElement) &&
                    titleElement.ValueKind == JsonValueKind.String)
                {
                    return titleElement.GetString();
                }
            }
        }
        catch (JsonException)
        {
            // If the response is plain text, return as-is.
        }

        return raw;
    }
}
