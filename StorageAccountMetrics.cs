namespace Azure.StorageMetrics
{
    using Azure.Identity;
    using Azure.Monitor.Query;
    using Azure.Monitor.Query.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class StorageAccountMetrics
    {
        private MetricsQueryClient client;
        private string resourceId;

        public StorageAccountMetrics(string tenantId, string clientId, string clientSecret, string subscriptionId, string resourceGroupName, string storageAccountName)
        {
            client = new MetricsQueryClient(new DefaultAzureCredential());
            resourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Storage/storageAccounts/{storageAccountName}";
        }

        /// <summary>
        /// Retrieves Azure Storage Account blob container storage consumption metrics for the specified time range and granularity.
        /// </summary>
        /// <param name="startDateTime">Start date and time.</param>
        /// <param name="endDateTime">End date and time.</param>
        /// <param name="granularity">Time span.</param>
        /// <returns>List of metrics results.</returns>
        /// <example>
        /// RetrieveMetricsAsync(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, TimeSpan.FromHours(1)).Wait();
        /// </example>
        public async Task<List<MetricResult>> RetrieveMetricsAsync(DateTimeOffset startDateTime, DateTimeOffset endDateTime, TimeSpan granularity)
        {
            var response = await client.QueryResourceAsync(
                resourceId,
                new[] { "BlobCapacity", "BlobCount" },
                new MetricsQueryOptions
                {
                    TimeRange = new QueryTimeRange(startDateTime, endDateTime),
                    Granularity = granularity
                }
            );

            return response.Value.Metrics.ToList();
        }
    }
}
