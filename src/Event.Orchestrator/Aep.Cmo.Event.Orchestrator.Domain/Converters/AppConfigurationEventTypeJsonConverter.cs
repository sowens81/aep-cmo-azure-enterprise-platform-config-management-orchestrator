using Aep.Cmo.Event.Orchestrator.Domain.Enum;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aep.Cmo.Event.Orchestrator.Domain.Converters;

public sealed class AppConfigurationEventTypeJsonConverter
    : JsonConverter<AppConfigurationEventType>
{
    public override AppConfigurationEventType Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();

        return value switch
        {
            "Microsoft.AppConfiguration.KeyValueModified"
                => AppConfigurationEventType.KeyValueModified,

            "Microsoft.AppConfiguration.KeyValueDeleted"
                => AppConfigurationEventType.KeyValueDeleted,

            _ => throw new JsonException(
                $"Unknown App Configuration eventType '{value}'.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        AppConfigurationEventType value,
        JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            AppConfigurationEventType.KeyValueModified
                => "Microsoft.AppConfiguration.KeyValueModified",

            AppConfigurationEventType.KeyValueDeleted
                => "Microsoft.AppConfiguration.KeyValueDeleted",

            _ => throw new JsonException(
                $"Unsupported AppConfigurationEventType '{value}'.")
        };

        writer.WriteStringValue(stringValue);
    }
}
