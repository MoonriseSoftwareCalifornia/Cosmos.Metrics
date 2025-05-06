namespace Cosmos.Metrics
{
    using Azure;
    using Azure.Identity;
    using Azure.Monitor.Query;
    using Azure.Monitor.Query.Models;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Gets statistics from an Azure Front Door profile.
    /// </summary>
    public class FrontDoorProfileMetrics
    {
        private readonly LogsQueryClient client;
        private readonly string workspaceId;
        private readonly string query = @"
            AzureDiagnostics
            | where ResourceProvider == 'MICROSOFT.CDN' and Category == 'FrontDoorAccessLog'
            | summarize RequestBytes = sum(tolong(requestBytes_s)), ResponseBytes = sum(tolong(responseBytes_s)) by Date = format_datetime(endofday(bin(TimeGenerated, 1d)), 'yyyy-MM-dd'), Hostname = hostName_s";

        /// <summary>
        ///  Creates an instance of the FrontDoorEndpointMetrics class with the specified parameters.
        /// </summary>
        /// <param name="subscriptionId">Subscription ID.</param>
        /// <param name="resourceGroup">Resource group name.</param>
        /// <param name="frontDoorProfileName">Front door profile name.</param>
        /// <param name="endpointName"></param>
        public FrontDoorProfileMetrics(Guid logAnalyticsWorkspaceId)
        {
            client = new LogsQueryClient(new DefaultAzureCredential());
            workspaceId = logAnalyticsWorkspaceId.ToString();
        }


        /// <summary>
        /// Creates an instance of the FrontDoorProfileMetrics class with the specified parameters.
        /// </summary>
        /// <param name="logAnalyticsWorkspaceId">Log analytics workspace ID</param>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="clientId">Registerd App. Client Id.</param>
        /// <param name="clientSecret">Registered App. secret.</param>
        public FrontDoorProfileMetrics(Guid logAnalyticsWorkspaceId, string tenantId, string clientId, string clientSecret)
        {
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            client = new LogsQueryClient(credential);
            workspaceId = logAnalyticsWorkspaceId.ToString();
        }

        /// <summary>
        /// Retrieves FrontDoor egress metrics for the specified time range.
        /// </summary>
        /// <param name="startDateTime">Start date and time.</param>
        /// <param name="endDateTime">End date and time</param>
        /// <param name="hostName">Filter on a particular host name (optional).</param>
        /// <returns></returns>
        public async Task<List<EndPointMetric>> RetrieveMetricsAsync(DateTimeOffset startDateTime, DateTimeOffset endDateTime, string hostName = "")
        {
            var q = query;

            if (!string.IsNullOrEmpty(hostName))
            {
                q += q.Replace("Category == 'FrontDoorAccessLog'", $"Category == 'FrontDoorAccessLog' and hostName_s == '{hostName}'");
            }

            Response<LogsQueryResult> response = await client.QueryWorkspaceAsync(
            workspaceId,
            q,
            new QueryTimeRange(startDateTime, endDateTime));

            var data = new List<EndPointMetric>();

            foreach (var table in response.Value.AllTables)
            {
                foreach (var row in table.Rows)
                {
                    data.Add( new EndPointMetric
                    {
                        Date = DateOnly.FromDateTime(DateTime.Parse(row["Date"].ToString())),
                        ResponseBytes = row["ResponseBytes"] == null ? 0 : long.Parse(row["ResponseBytes"].ToString()),
                        RequestBytes = row["RequestBytes"] == null ? 0 : long.Parse(row["RequestBytes"].ToString()),
                        Host = row["Hostname"] == null ? "" : row["Hostname"].ToString()
                    });
                }
            }

            return data;
        }
    }

}
