using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BankProductsRegistry.Api.Utilities;

public static class ValidationMessageTranslator
{
    public static IDictionary<string, string[]> Translate(ModelStateDictionary modelState)
    {
        return modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => GetFieldName(entry.Key),
                entry => entry.Value!.Errors
                    .Select(error => TranslateMessage(error.ErrorMessage))
                    .Distinct()
                    .ToArray());
    }

    private static string GetFieldName(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return "general";
        }

        var normalizedKey = key.Split('.').Last();
        return normalizedKey;
    }

    private static string TranslateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Valor no valido.";
        }

        if (message.Contains("A non-empty request body is required.", StringComparison.OrdinalIgnoreCase))
        {
            return "Debes enviar informacion en la solicitud.";
        }

        if (message.Contains("The JSON value could not be converted", StringComparison.OrdinalIgnoreCase))
        {
            return "Uno de los valores enviados no tiene el formato correcto.";
        }

        if (message.Contains("field is required", StringComparison.OrdinalIgnoreCase))
        {
            return "Este campo es obligatorio.";
        }

        if (message.Contains("valid e-mail address", StringComparison.OrdinalIgnoreCase))
        {
            return "Debes escribir un correo valido.";
        }

        var exactLengthMatch = Regex.Match(
            message,
            @"minimum length of '(\d+)'.*maximum length of '(\d+)'",
            RegexOptions.IgnoreCase);

        if (exactLengthMatch.Success && exactLengthMatch.Groups[1].Value == exactLengthMatch.Groups[2].Value)
        {
            return $"Debe tener exactamente {exactLengthMatch.Groups[1].Value} caracteres.";
        }

        var maxLengthMatch = Regex.Match(
            message,
            @"maximum length of '(\d+)'",
            RegexOptions.IgnoreCase);

        if (maxLengthMatch.Success)
        {
            return $"No puede tener mas de {maxLengthMatch.Groups[1].Value} caracteres.";
        }

        var rangeMatch = Regex.Match(
            message,
            @"between ([\d\.\-]+) and ([\d\.\-]+)",
            RegexOptions.IgnoreCase);

        if (rangeMatch.Success)
        {
            return $"Debe estar entre {rangeMatch.Groups[1].Value} y {rangeMatch.Groups[2].Value}.";
        }

        return "Uno o mas campos tienen valores no validos.";
    }
}
