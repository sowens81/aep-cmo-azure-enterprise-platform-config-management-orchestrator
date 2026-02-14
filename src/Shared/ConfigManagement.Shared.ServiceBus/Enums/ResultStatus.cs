namespace ConfigManagement.Shared.ServiceBus.Enums;

/// <summary>
/// Represents the outcome status of a Service Bus operation or message processing result.
/// </summary>
/// <remarks>
/// This enumeration can be used to standardize result reporting across
/// message handlers, publishers, or processing pipelines.
/// </remarks>
public enum ResultStatus
{
    /// <summary>
    /// Indicates that the operation completed successfully
    /// without errors or warnings.
    /// </summary>
    SUCCESS,

    /// <summary>
    /// Indicates that the operation failed
    /// and did not complete as expected.
    /// </summary>
    FAILED,

    /// <summary>
    /// Indicates that the operation completed,
    /// but with non-critical issues or warnings.
    /// </summary>
    WARNING
}
