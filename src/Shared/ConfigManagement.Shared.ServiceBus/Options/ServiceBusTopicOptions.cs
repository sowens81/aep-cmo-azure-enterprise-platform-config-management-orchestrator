using ConfigManagement.Shared.ServiceBus.Interfaces;

namespace ConfigManagement.Shared.ServiceBus.Options;

/// <summary>
/// Represents configuration options for an Azure Service Bus topic.
/// </summary>
/// <remarks>
/// This options class is typically bound from configuration (e.g., appsettings.json)
/// and defines the topic name used for publishing or subscribing to messages.
///
/// All properties use <c>init</c> accessors to ensure immutability after
/// configuration binding.
/// </remarks>
public class ServiceBusTopicOptions : IServiceBusTopicOptions
{
    /// <summary>
    /// Gets the name of the Azure Service Bus topic.
    /// </summary>
    /// <remarks>
    /// The topic must already exist in the configured Service Bus namespace.
    /// This value is case-sensitive depending on the Service Bus configuration.
    /// </remarks>
    public string TopicName { get; init; } = default!;
}
