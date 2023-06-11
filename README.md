Working with Azure Cosmos DB in your Azure Functions

### What is Cosmos DB
Azure Cosmos DB is a powerful and scalable database service provided by Microsoft Azure. It is designed to handle globally distributed data to handle large-scale, high-performance applications that require low latency. Cosmos DB is a NoSQL database supporting multiple data models, documents, key-value, graphs, and tabular storage.

With Cosmos DB, you can store and retrieve data using a variety of APIs, including SQL (Document DB), MongoDB, Cassandra, Gremlin, and Azure Table Storage. This flexibility allows you to work with Cosmos DB using familiar programming models and tools. It also offers features like automatic indexing, automatic secondary indexing, and built-in geospatial and temporal data support.

> **Geospatial** data can include various types of information, such as coordinates, elevation, demographics, land use, transportation networks, weather patterns, and more. 

#### Setup Cosmos in locally (Windows)

1. Download and Install Azure Cosmos DB Emulator:
     - Visit the Azure Cosmos DB Emulator page: https://aka.ms/cosmosdb-emulator
	 - Click on the "Download" button to download the emulator installer.
	 - Run the downloaded installer and follow the installation wizard to complete the installation.
2. Start the Azure Cosmos DB Emulator:
	 - After the installation is complete, open the Azure Cosmos DB Emulator from the Start menu or desktop shortcut.
	 - The emulator will start, and you should see a green icon in the system tray indicating that it's running.
	 
3. Access the Azure Cosmos DB Emulator Explorer:
	 - Open a web browser and navigate to the following URL: https://localhost:8081/_explorer/index.html
	 - This will open the Azure Cosmos DB Emulator Explorer, which provides a web interface to interact with the emulator.
	 
4. Connect to the Emulator from your application:

	 - In your application code, you can use the Azure Cosmos DB SDKs to connect to the emulator.
	 - Use the following connection details:
	 - Endpoint URI: https://localhost:8081 and Primary Key

**Note:** These connection details are specific to the Azure Cosmos DB Emulator and should not be used in production.

#### Setup Cosmos in Azure

1. Create an Azure Cosmos DB Account:
	 - Choose the appropriate API based on your data model needs (e.g., SQL, MongoDB, Cassandra, etc.).
	 - Configure the desired consistency level, geo-replication options, and pricing tier.
	 - Click on the "Review + Create" button and then "Create" to create the Cosmos DB account.

![Azure Cosmos Create](https://www.dropbox.com/s/yirqllkwg8i6wq0/azure_create_review.png?raw=1 "Azure Cosmos Create")	 

### Create Database & Container

1. Create a Database and Collection:
	 - In the Cosmos DB account overview page, click on the "Data Explorer" tab.
	 - Click on "New Container" to create a new database and collection.
	 - Provide the required information such as database ID, collection ID, partition key, etc.
2. Configure the desired indexing policy and throughput (request units per second).
3. Click on "OK" to create the new database and collection.

![Cosmos Container](https://www.dropbox.com/s/oaq7r56m3jg4cgh/cosmos_container.png?raw=1 "Cosmos Container")
### Add Records Manually in Container 

1. In Data Explorer, expand the `Students` database and the Items container.
2. Next, select Items, and then select New Item.
3. Add the following structure to the document on the right side of the Documents pane:
4. Select `Save`.

![Save](https://www.dropbox.com/s/5xog5pv6p3qikug/save_manually_azure.png?raw=1 "Save")

### Let's create something real time.

**Insert Record Function:**

- The Insert Record function, as described in the following code snippet, handles the HTTP POST request to insert a student record into a Cosmos DB collection.
- When a POST request is received, the function reads the request body, deserializes it into a StudentEntity object, and creates a new document in the specified Cosmos DB collection with the student record data.
- The function returns the inserted document as the response.

**Change Feed Function:**

- The Change Feed function, as described in the following code snippet, uses the Cosmos DB trigger to monitor changes in a specific Cosmos DB collection.
- When a change occurs, such as an insertion, update, or deletion of a document in the monitored collection, the Change Feed function is triggered.
- The function receives the batch of changed documents as the input parameter and can perform custom logic to process the changed documents.

**Relationship between the functions:**

- After the Insert Record function inserts a new student record into the Cosmos DB collection, the Change Feed function can be triggered if the insertion causes a change in the monitored collection.
- The Change Feed function can then process the inserted document or perform any desired logic based on the change.
For example, you can use the Change Feed function to send notifications, update related data, or trigger other actions whenever a new student record is inserted.

![Demo](https://www.dropbox.com/s/1kvleq66b0xk3oh/cover_page1.png?raw=1 "Demo")


> The full source code for this article can be found on [GitHub](https://github.com/engg-aruny/CodeHackCosmosTriggerFunctionApp).

### Add Record Using HTTP Trigger Function
The code handles the HTTP POST request, deserializes the request body into a StudentEntity object, creates a document with the student data, and returns the document as the response. It also performs error handling and logging using the provided ILogger instance.

Note that you need to replace the values for databaseName, collectionName, and ConnectionStringSetting with your specific Cosmos DB configuration.

```csharp
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

```
### Cosmos Trigger Function to track changes in a Cosmos DB Collection
Change Feed functions are useful for scenarios where you need to react to changes in your Cosmos DB data. It allows you to process and react to changes in real-time, enabling scenarios like data synchronization, notifications, analytics, or data transformations.

By leveraging the Change Feed feature and an Azure Function, you can build event-driven architectures and automate workflows based on changes in your Cosmos DB data.

```csharp
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
```
### Summary with Output

In summary, By combining the Insert Record function and the Change Feed function, you can achieve a workflow where new records are inserted into Cosmos DB, and you can react to those insertions or changes in real-time using the Change Feed function. This allows for more dynamic and event-driven data processing and automation based on the changes happening in your Cosmos DB collection.

![Insert Record With Change Feed](https://www.dropbox.com/s/g0uw3rxzxiwv680/output_function.png?raw=1 "Insert Record With Change Feed")




