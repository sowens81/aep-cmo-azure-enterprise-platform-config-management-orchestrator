namespace Aep.Cmo.Shared.Domain;

/// <summary>
/// Represents a void result in a generic context.
/// </summary>
public readonly struct Unit
{
    public static readonly Unit Value = new();
}
