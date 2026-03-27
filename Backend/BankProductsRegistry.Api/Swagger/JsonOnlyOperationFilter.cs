using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BankProductsRegistry.Api.Swagger;

public sealed class JsonOnlyOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody is not null && operation.RequestBody.Content.Count > 0)
        {
            var preferredRequestMediaType = GetPreferredMediaType(operation.RequestBody.Content);
            if (preferredRequestMediaType is not null)
            {
                operation.RequestBody.Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = preferredRequestMediaType
                };
            }
        }

        foreach (var response in operation.Responses.Values)
        {
            if (response.Content.Count == 0)
            {
                continue;
            }

            var preferredResponseMediaType = GetPreferredMediaType(response.Content);
            if (preferredResponseMediaType is null)
            {
                continue;
            }

            var mediaTypeName = IsProblemSchema(preferredResponseMediaType.Schema)
                ? "application/problem+json"
                : "application/json";

            response.Content = new Dictionary<string, OpenApiMediaType>
            {
                [mediaTypeName] = preferredResponseMediaType
            };
        }
    }

    private static OpenApiMediaType? GetPreferredMediaType(IDictionary<string, OpenApiMediaType> content)
    {
        if (content.TryGetValue("application/json", out var jsonMediaType))
        {
            return jsonMediaType;
        }

        if (content.TryGetValue("application/problem+json", out var problemMediaType))
        {
            return problemMediaType;
        }

        return content.Values.FirstOrDefault();
    }

    private static bool IsProblemSchema(OpenApiSchema? schema)
    {
        var schemaId = schema?.Reference?.Id ?? schema?.Title;
        return string.Equals(schemaId, nameof(ProblemDetails), StringComparison.Ordinal) ||
               string.Equals(schemaId, nameof(ValidationProblemDetails), StringComparison.Ordinal);
    }
}
