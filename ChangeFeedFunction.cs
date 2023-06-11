using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace CodeHackCosmosTriggerFunctionApp
{
    public static class ChangeFeedFunction
    {
        [FunctionName("ChangeFeed")]
        public static void Run([CosmosDBTrigger(
            databaseName: "SchoolDB",
            collectionName: "Students",
            ConnectionStringSetting = "CosmosDBConnection",
            CreateLeaseCollectionIfNotExists = true
            )]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
            }
        }
    }
}
