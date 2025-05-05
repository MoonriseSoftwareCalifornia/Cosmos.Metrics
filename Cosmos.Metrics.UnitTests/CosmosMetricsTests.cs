using Azure.StorageMetrics;
using Microsoft.Extensions.Configuration;

namespace Cosmos.Metrics.UnitTests
{
    [TestClass]
    public sealed class CosmosMetricsTests
    {
        private static IConfigurationRoot LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .AddUserSecrets<CosmosMetricsTests>(optional:false, reloadOnChange: true)
                .Build();
        }

        [TestMethod]
        public void CosmosDBMetrics_Test()
        {
            var configuration = LoadConfiguration();
            var clientSecret = configuration["ClientSecret"];
            var clientId = configuration["ClientId"];
            var tenantId = configuration["TenantId"];
            var subscriptionId = configuration["SubscriptionId"];
            var resourceGroupName = configuration["ResourceGroupName"];

            var cosmosMetrics = new CosmosDBMetrics(
                subscriptionId: subscriptionId,
                resourceGroup: resourceGroupName,
                accountName: "cosmosr33vq5a76ct7m",
                tenantId: tenantId,
                clientId: clientId,
                clientSecret: clientSecret
            );

            var results = cosmosMetrics.RetrieveMetricsAsync(
                startDateTime: DateTime.UtcNow.AddDays(-30),
                endDateTime: DateTime.UtcNow,
                granularity: TimeSpan.FromHours(1)
            ).Result;

            Assert.IsNotNull(results);

            var r = results
                .Where(x => x.ResourceName == "TotalRequestUnits");

            var sumRus = r.Sum(x => x.Total);

            Assert.IsTrue(sumRus > 0);
        }

        [TestMethod]
        public void CosmosStorageMetrics_Test()
        {
            var configuration = LoadConfiguration();
            var clientSecret = configuration["ClientSecret"];
            var clientId = configuration["ClientId"];
            var tenantId = configuration["TenantId"];
            var subscriptionId = configuration["SubscriptionId"];
            var resourceGroupName = configuration["ResourceGroupName"];

            var cosmosMetrics = new StorageAccountMetrics(
                subscriptionId: subscriptionId,
                resourceGroup: resourceGroupName,
                accountName: "filesr33vq5a76ct7m",
                tenantId: tenantId,
                clientId: clientId,
                clientSecret: clientSecret
            );

            var results = cosmosMetrics.RetrieveMetricsAsync(
                startDateTime: DateTime.UtcNow.AddDays(-30),
                endDateTime: DateTime.UtcNow,
                granularity: TimeSpan.FromHours(1)
            ).Result;

            Assert.IsNotNull(results);

            var sumBytes = results
                .Where(x => x.ResourceName == "UsedCapacity")
                .Max(x => x.Total);

            var sumEgress = results
                .Where(x => x.ResourceName == "Egress")
                .Max(x => x.Total);

            var sumIngress = results.Where(x => x.ResourceName == "Ingress")
                .Max(x => x.Total);

            Assert.IsTrue(sumBytes > 0);
            Assert.IsTrue(sumEgress > 0);
            Assert.IsTrue(sumIngress > 0);
        }

        [TestMethod]
        public void CosmosFrontDoorProfileMetrics_Test()
        {
            var configuration = LoadConfiguration();
            var clientSecret = configuration["ClientSecret"];
            var clientId = configuration["ClientId"];
            var tenantId = configuration["TenantId"];
            var subscriptionId = configuration["SubscriptionId"];
            var resourceGroupName = configuration["ResourceGroupName"];

            var cosmosMetrics = new FrontDoorProfileMetrics(
                logAnalyticsWorkspaceId: Guid.Parse("b56f98ef-faf3-4c69-9df1-d683b4505247"),
                tenantId: tenantId,
                clientId: clientId,
                clientSecret: clientSecret
            );

            var results = cosmosMetrics.RetrieveMetricsAsync(
                startDateTime: DateTime.UtcNow.AddDays(-30),
                endDateTime: DateTime.UtcNow).Result;

            Assert.IsNotNull(results);

            var sumBytes = results.Sum(s => s.Bytes);

            var hostCount = results.Select(s => s.Host).Distinct().Count();

            var days = results.Select(s => s.Date).Distinct().Count();

            Assert.IsTrue(sumBytes > 0);
            Assert.IsTrue(hostCount > 0);
            Assert.IsTrue(days > 0);
        }
    }
}
