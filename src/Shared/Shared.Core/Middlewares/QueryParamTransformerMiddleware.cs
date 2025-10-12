using System.Web;
using Microsoft.AspNetCore.Http;

namespace Shared.Core.Middlewares;

public class QueryParameterTransformerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.QueryString.HasValue)
        {
            var transformedQuery = TransformQueryString(context.Request.QueryString.Value);

            context.Request.QueryString = new QueryString(transformedQuery);
        }

        await next(context);
    }

    private static string TransformQueryString(string originalQuery)
    {
        // Remove o '?' inicial se existir
        var query = originalQuery.TrimStart('?');

        if (string.IsNullOrEmpty(query))
            return originalQuery;

        var queryParams = HttpUtility.ParseQueryString(query);
        var transformedParams = new List<string>();

        foreach (string key in queryParams.Keys)
        {
            if (key == null) continue;

            var values = queryParams.GetValues(key);

            if (values != null)
                transformedParams.AddRange(values.Select(ConvertSnakeCaseToPascalCase)
                    .Select(transformedValue => $"{key}={HttpUtility.UrlEncode(transformedValue)}"));
        }

        return transformedParams.Count > 0 ? "?" + string.Join("&", transformedParams) : originalQuery;
    }

    private static string ConvertSnakeCaseToPascalCase(string snakeCase)
    {
        if (string.IsNullOrEmpty(snakeCase))
            return snakeCase;

        if (!snakeCase.Contains('_')) return char.ToUpperInvariant(snakeCase[0]) + snakeCase[1..].ToLowerInvariant();

        var parts = snakeCase.Split('_', StringSplitOptions.RemoveEmptyEntries);
        var result = string.Join("", parts.Select(part =>
            char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));

        return result;
    }
}