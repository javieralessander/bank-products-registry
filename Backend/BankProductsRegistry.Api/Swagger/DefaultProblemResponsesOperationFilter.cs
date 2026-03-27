using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BankProductsRegistry.Api.Swagger;

public sealed class DefaultProblemResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        var isAnonymous = endpointMetadata.OfType<IAllowAnonymous>().Any();
        var hasAuthorize = endpointMetadata.OfType<IAuthorizeData>().Any();

        if (operation.RequestBody is not null)
        {
            AddProblemResponse(operation, context, StatusCodes.Status400BadRequest, typeof(ValidationProblemDetails));
        }

        if (!isAnonymous && hasAuthorize)
        {
            AddProblemResponse(operation, context, StatusCodes.Status401Unauthorized, typeof(ProblemDetails));
            AddProblemResponse(operation, context, StatusCodes.Status403Forbidden, typeof(ProblemDetails));
        }

        AddProblemResponse(operation, context, StatusCodes.Status500InternalServerError, typeof(ProblemDetails));
    }

    private static void AddProblemResponse(
        OpenApiOperation operation,
        OperationFilterContext context,
        int statusCode,
        Type schemaType)
    {
        var statusCodeText = statusCode.ToString();
        if (operation.Responses.ContainsKey(statusCodeText))
        {
            return;
        }

        var schema = context.SchemaGenerator.GenerateSchema(schemaType, context.SchemaRepository);
        operation.Responses[statusCodeText] = new OpenApiResponse
        {
            Description = GetDescription(statusCode),
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new()
                {
                    Schema = schema
                }
            }
        };
    }

    private static string GetDescription(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            _ => "Internal Server Error"
        };
}
