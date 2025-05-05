namespace Azure.StorageMetrics
{
    using Azure.Identity;
    using Azure.Monitor.Query;
    using Azure.Monitor.Query.Models;
    using Cosmos.Metrics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class StorageAccountMetrics
    {
        private MetricsQueryClient client;
        private string resourceId;

        /// <summary>
        /// Creates an instance of the StorageAccountMetrics class with the specified parameters.
        /// </summary>
        /// <param name="subscriptionId">Subscription ID.</param>
        /// <param name="resourceGroup">Resource group name.</param>
        /// <param name="accountName">Storage account name.</param>
        public StorageAccountMetrics(string subscriptionId, string resourceGroup, string accountName)
        {
            client = new MetricsQueryClient(new DefaultAzureCredential());
            resourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{accountName}";
        }

        /// <summary>
        /// Creates an instance of the StorageAccountMetrics class with the specified parameters.
        /// </summary>
        /// <param name="defaultAzureCredential">An existing Azure Default Credential.</param>
        /// <param name="subscriptionId">Subscription ID.</param>
        /// <param name="resourceGroup">Resource group name.</param>
        /// <param name="accountName">Storage account name.</param>
        public StorageAccountMetrics(DefaultAzureCredential defaultAzureCredential, string subscriptionId, string resourceGroup, string accountName)
        {
            client = new MetricsQueryClient(defaultAzureCredential);
            resourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{accountName}";
        }

        /// <summary>
        ///  Creates an instance of the StorageAccountMetrics class with the specified parameters.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="clientId">Registered application client ID.</param>
        /// <param name="clientSecret">Registered application secret value.</param>
        /// <param name="subscriptionId">Subscription ID.</param>
        /// <param name="resourceGroup">Resource group name.</param>
        /// <param name="accountName">Storage account name.</param>
        public StorageAccountMetrics(string tenantId, string clientId, string clientSecret, string subscriptionId, string resourceGroup, string accountName)
        {
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            client = new MetricsQueryClient(credential);
            resourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{accountName}";
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
        public async Task<List<QueryResultMetric>> RetrieveMetricsAsync(DateTimeOffset startDateTime, DateTimeOffset endDateTime, TimeSpan granularity)
        {
            var options = new MetricsQueryOptions();
            options.TimeRange = new QueryTimeRange(startDateTime, endDateTime);
            options.Granularity = granularity;
            options.Aggregations.Clear();
            options.Aggregations.Add(MetricAggregationType.Maximum);
            options.Aggregations.Add(MetricAggregationType.Total);

            var response = await client.QueryResourceAsync(
                resourceId,
                new[] { "UsedCapacity", "Transactions", "Egress", "Ingress" },
                options
            );


            var results = new List<QueryResultMetric>();

            foreach (var metric in response.Value.Metrics)
            {
                foreach (var timeSeriesElement in metric.TimeSeries)
                {
                    foreach (var data in timeSeriesElement.Values)
                    {
                        var met = new QueryResultMetric();
                        met.MetricName = metric.Unit.ToString();
                        met.ResourceType = metric.ResourceType;
                        met.ResourceId = metric.Id;
                        met.ResourceName = metric.Name;
                        met.TimeStamp = data.TimeStamp;
                        met.Total = data.Total;
                        met.Minimum = data.Minimum;
                        met.Maximum = data.Maximum;
                        met.Average = data.Average;
                        met.Count = data.Count;
                        results.Add(met);
                    }
                }
            }

            return results;
        }
    }
}