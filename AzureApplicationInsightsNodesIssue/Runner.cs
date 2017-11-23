using System;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace AzureApplicationInsightsNodesIssue
{
    public static class Runner
    {
        private static readonly TelemetryClient TelemetryClient = new TelemetryClient
        {
            InstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process),
            Context =
            {
                Cloud =
                {
                    RoleInstance = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME", EnvironmentVariableTarget.Process),
                    RoleName = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME", EnvironmentVariableTarget.Process)
                }
            }
        };

        [FunctionName("Send")]
        public static async Task SendAsync([TimerTrigger("*/10 * * * * *")] TimerInfo myTimer, [Queue("messages")] IAsyncCollector<string> queue, TraceWriter log)
        {
            var message = DateTimeOffset.UtcNow.ToString();

            for (var i = 0; i < 1000; i++)
            {
                await queue.AddAsync(message);
            }

            log.Info($"[TraceWriter] Sent: {message}");
            TelemetryClient.TrackEvent($"[TelemetryClient] Sent: {message}");
        }

        [FunctionName("Receive")]
        public static async Task ReceiveAsync([QueueTrigger("messages")] string message, TraceWriter log)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            log.Info($"[TraceWriter] Received: {message}");
            TelemetryClient.TrackEvent($"[TelemetryClient] Received: {message}");
        }
    }
}
