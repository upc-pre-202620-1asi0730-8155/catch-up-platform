using CatchUpPlatform.API.News.Interfaces.Rest.Resources;
using CatchUpPlatform.API.Resources;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi;

namespace CatchUpPlatform.API.News.Interfaces.Rest.Transform;

/// <summary>
///     Assembles localized OpenAPI metadata (summaries, descriptions, and response texts)
///     for the News REST endpoints and resources.
/// </summary>
public static class LocalizedOpenApiAssembler
{
    private static readonly Dictionary<string, (string Summary, string Description,
        IReadOnlyDictionary<string, string> Responses)> OperationResources =
        new Dictionary<string, (string, string, IReadOnlyDictionary<string, string>)>
        {
            ["CreateFavoriteSource"] = (
                "CreateFavoriteSourceSummary",
                "CreateFavoriteSourceDescription",
                new Dictionary<string, string>
                {
                    ["201"] = "CreateFavoriteSourceCreated",
                    ["400"] = "CreateFavoriteSourceBadRequest",
                    ["409"] = "CreateFavoriteSourceConflict",
                    ["500"] = "CreateFavoriteSourceServerError"
                }),
            ["GetFavoriteSourceFromQuery"] = (
                "GetFavoriteSourceQuerySummary",
                "GetFavoriteSourceQueryDescription",
                new Dictionary<string, string>
                {
                    ["200"] = "GetFavoriteSourceQuerySuccess",
                    ["400"] = "GetFavoriteSourceQueryBadRequest",
                    ["404"] = "GetFavoriteSourceQueryNotFound"
                }),
            ["GetFavoriteSourceById"] = (
                "GetFavoriteSourceByIdSummary",
                "GetFavoriteSourceByIdDescription",
                new Dictionary<string, string>
                {
                    ["200"] = "GetFavoriteSourceByIdSuccess",
                    ["404"] = "GetFavoriteSourceByIdNotFound"
                })
        };

    /// <summary>
    ///     Localizes the summary, description, operation id and response descriptions of known operations.
    /// </summary>
    /// <param name="operation">The OpenAPI operation being transformed.</param>
    /// <param name="context">The transformer context, used to resolve the localizer and action descriptor.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public static Task TransformOperationAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor action ||
            !OperationResources.TryGetValue(action.ActionName, out var resources))
            return Task.CompletedTask;

        var localizer = context.ApplicationServices.GetRequiredService<IStringLocalizer<SharedResource>>();
        operation.OperationId = action.ActionName;
        operation.Summary = localizer[resources.Summary].Value;
        operation.Description = localizer[resources.Description].Value;
        if (operation.Responses is null) return Task.CompletedTask;

        foreach (var (statusCode, resourceKey) in resources.Responses)
        {
            if (operation.Responses.TryGetValue(statusCode, out var response) &&
                response is OpenApiResponse openApiResponse)
                openApiResponse.Description = localizer[resourceKey].Value;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Localizes the description of known resource schemas and their properties.
    /// </summary>
    /// <param name="schema">The OpenAPI schema being transformed.</param>
    /// <param name="context">The transformer context, used to resolve the localizer and type information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public static Task TransformSchemaAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var localizer = context.ApplicationServices.GetRequiredService<IStringLocalizer<SharedResource>>();
        var type = context.JsonTypeInfo.Type;
        if (type == typeof(CreateFavoriteSourceResource))
            schema.Description = localizer["CreateFavoriteSourceSchema"].Value;
        else if (type == typeof(FavoriteSourceResource))
            schema.Description = localizer["FavoriteSourceSchema"].Value;

        var propertyResourceKey = context.JsonPropertyInfo?.Name switch
        {
            "id" => "FavoriteSourceId",
            "newsApiKey" => "FavoriteSourceNewsApiKey",
            "sourceId" => "FavoriteSourceSourceId",
            _ => null
        };

        if (propertyResourceKey is not null)
            schema.Description = localizer[propertyResourceKey].Value;

        return Task.CompletedTask;
    }
}