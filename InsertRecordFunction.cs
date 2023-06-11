using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CodeHackCosmosTriggerFunctionApp
{
    public static class InsertRecordFunction
    {
        [FunctionName("InsertRecord")]
        public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "records")] HttpRequest req,
        [CosmosDB(
            databaseName: "SchoolDB",
            collectionName: "Students",
            ConnectionStringSetting = "CosmosDBConnection")] out StudentEntity document, ILogger log)
        {
            document = null;
            try
            {
                log.LogInformation("InsertRecord function processed a request.");

                string requestBody = req.ReadAsStringAsync().Result;
                var student = Newtonsoft.Json.JsonConvert.DeserializeObject<StudentEntity>(requestBody);

                var jsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                var result = JsonConvert.SerializeObject(student, jsonSettings);

                // Create a new document object with the record data
                document = new StudentEntity
                {
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Email = student.Email,
                    PhoneNumber = student.PhoneNumber
                };
            }
            catch (System.Exception ex)
            {

                log.LogError(ex.Message);
            }

            // Return a successful response with the inserted document
            return new OkObjectResult(document);
        }
    }


    public class StudentEntity
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
    }
}
