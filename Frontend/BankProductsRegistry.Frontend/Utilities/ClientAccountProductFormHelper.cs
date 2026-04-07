using System.Security.Claims;
using System.Text.Json;
using BankProductsRegistry.Frontend.Models;

namespace BankProductsRegistry.Frontend.Utilities;

/// <summary>
/// Autocompletado de cuenta contratada cuando el usuario tiene rol Cliente (solo ve sus productos).
/// </summary>
public static class ClientAccountProductFormHelper
{
    private static string BuildLabel(JsonElement a)
    {
        var clientName = a.GetProperty("clientName").GetString() ?? "";
        var productName = a.GetProperty("financialProductName").GetString() ?? "";
        var accountNumber = a.GetProperty("accountNumber").GetString() ?? "";
        return $"{clientName} — {productName} ({accountNumber})";
    }

    /// <summary>Cuentas en estado activo (avisos de viaje).</summary>
    public static List<(int Id, string Label)> ParseTravelActiveProducts(string json)
    {
        var list = new List<(int Id, string Label)>();
        using var doc = JsonDocument.Parse(json);
        foreach (var a in doc.RootElement.EnumerateArray())
        {
            var st = a.GetProperty("status").GetString();
            if (!string.Equals(st, "activo", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            list.Add((a.GetProperty("id").GetInt32(), BuildLabel(a)));
        }

        return list;
    }

    /// <summary>Cuentas que aún pueden bloquearse (no cerradas ni canceladas).</summary>
    public static List<(int Id, string Label)> ParseBlockEligibleProducts(string json)
    {
        var list = new List<(int Id, string Label)>();
        using var doc = JsonDocument.Parse(json);
        foreach (var a in doc.RootElement.EnumerateArray())
        {
            var st = a.GetProperty("status").GetString() ?? "";
            if (string.Equals(st, "cerrado", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(st, "cancelado", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            list.Add((a.GetProperty("id").GetInt32(), BuildLabel(a)));
        }

        return list;
    }

    private static void ApplySelectionCore(
        ClaimsPrincipal user,
        List<(int Id, string Label)> items,
        ref int accountProductId,
        out bool clientCardLocked,
        out string? clientCardDisplayLabel)
    {
        clientCardLocked = false;
        clientCardDisplayLabel = null;

        if (!user.IsInRole("Cliente") || items.Count == 0)
        {
            return;
        }

        if (items.Count == 1)
        {
            accountProductId = items[0].Id;
            clientCardLocked = true;
            clientCardDisplayLabel = items[0].Label;
            return;
        }

        if (accountProductId <= 0)
        {
            accountProductId = items[0].Id;
        }
    }

    public static void ApplyToTravelNotice(ClaimsPrincipal user, string accountProductsJson, TravelNoticeCreateViewModel model)
    {
        var items = ParseTravelActiveProducts(accountProductsJson);
        var id = model.AccountProductId;
        ApplySelectionCore(user, items, ref id, out var locked, out var label);
        model.AccountProductId = id;
        model.ClientCardLocked = locked;
        model.ClientCardDisplayLabel = label;
    }

    public static void ApplyToBlockCreate(ClaimsPrincipal user, string accountProductsJson, BlockCreateViewModel model)
    {
        var items = ParseBlockEligibleProducts(accountProductsJson);
        var id = model.AccountProductId;
        ApplySelectionCore(user, items, ref id, out var locked, out var label);
        model.AccountProductId = id;
        model.ClientCardLocked = locked;
        model.ClientCardDisplayLabel = label;
    }
}
