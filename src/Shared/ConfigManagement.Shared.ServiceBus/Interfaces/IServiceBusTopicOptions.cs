namespace ConfigManagement.Shared.ServiceBus.Interfaces;

/// <summary>
/// Defines configuration required for an Azure Service Bus topic.
/// </summary>
/// <remarks>
/// This interface is typically implemented by an options class that is bound
/// from configuration (e.g., appsettings.json).
/// It provides the topic name used by publishers and/or subscribers
/// when interacting with Azure Service Bus.
/// </remarks>
public interface IServiceBusTopicOptions
{
    /// <summary>
    /// Gets the name of the Azure Service Bus topic.
    /// </summary>
    /// <remarks>
    /// The topic must exist in the configured Service Bus namespace.
    /// </remarks>
    string TopicName { get; }
}
