using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Core.Extensions;

public static class JsonExtensions
{
    public static void ConfigureJsonOptions(JsonSerializerOptions target, JsonSerializerOptions? source)
    {
        source ??= Configuration.Options;

        target.AllowTrailingCommas = source.AllowTrailingCommas;
        target.PropertyNameCaseInsensitive = source.PropertyNameCaseInsensitive;
        target.PropertyNamingPolicy = source.PropertyNamingPolicy;
        target.WriteIndented = source.WriteIndented;
        target.DefaultIgnoreCondition = source.DefaultIgnoreCondition;

        foreach (var converter in source.Converters)
            target.Converters.Add(converter);
    }

    public static class Configuration
    {
        public static readonly JsonSerializerOptions Options;

        static Configuration()
        {
            Options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            Options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
        }
    }
}