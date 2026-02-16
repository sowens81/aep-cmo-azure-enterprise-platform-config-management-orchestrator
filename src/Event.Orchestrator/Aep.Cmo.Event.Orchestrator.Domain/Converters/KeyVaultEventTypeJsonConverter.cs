using Aep.Cmo.Event.Orchestrator.Domain.Enum;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aep.Cmo.Event.Orchestrator.Domain.Converters;

public sealed class KeyVaultEventTypeJsonConverter
    : JsonConverter<KeyVaultEventType>
{
    public override KeyVaultEventType Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();

        return value switch
        {
            "Microsoft.KeyVault.SecretNewVersionCreated"
                => KeyVaultEventType.SecretNewVersionCreated,

            _ => throw new JsonException(
                $"Unknown Key Vault eventType '{value}'.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        KeyVaultEventType value,
        JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            KeyVaultEventType.SecretNewVersionCreated
                => "Microsoft.KeyVault.SecretNewVersionCreated",

            _ => throw new JsonException(
                $"Unsupported KeyVaultEventType '{value}'.")
        };

        writer.WriteStringValue(stringValue);
    }
}
