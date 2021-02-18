using Azure.Messaging.Replication;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EventHubToEventHubMerge
{
	public static class Tasks
    {
        const string remoteEventHubName = "telemetry";
        const string localEventHubName = "telemetry";
        const string localEventHubConnection = "LocalEventHubConnection";
        const string remoteEventHubConnection = "RemoteEventHubConnection";
        const string localEventHubConsumerGroup = "%LocalConsumerGroupName%";
        
        [FunctionName(nameof(ReplicateEventHub))]
        [ExponentialBackoffRetry(-1, "00:00:05", "00:05:00")]
        public static Task ReplicateEventHub(
            [EventHubTrigger(localEventHubName, ConsumerGroup = localEventHubConsumerGroup, Connection = localEventHubConnection)] EventData[] input,
            [EventHub(remoteEventHubName, Connection = remoteEventHubConnection)] EventHubClient outputClient,
            ILogger log)
        {
            return EventHubReplicationTasks.ConditionalForwardToEventHub(input, outputClient, log, (inputEvent) => {
                if (!inputEvent.Properties.ContainsKey("repl-target") || !string.Equals(inputEvent.Properties["repl-target"] as string, localEventHubName)) {
                      inputEvent.Properties["repl-target"] = remoteEventHubName;
                      return inputEvent;
                }
                return null;
            });
        }
    }
}
