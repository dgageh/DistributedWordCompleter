using Microsoft.Azure.Cosmos;

class Program
{
    private static readonly string EndpointUri = "https://localhost:8081/";
    private static readonly string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
    private static readonly string DatabaseId = "WordsDB";
    private static readonly string ContainerId = "WordsContainer";

    static async Task Main()
    {
        string cosmosEndpoint = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT") ?? EndpointUri;
        string cosmosKey = Environment.GetEnvironmentVariable("COSMOS_KEY") ?? PrimaryKey;

        var cosmosClient = new CosmosClient(cosmosEndpoint, cosmosKey);
        var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId);
        await database.Database.CreateContainerIfNotExistsAsync(ContainerId, "/id");

        Console.WriteLine("CosmosDB WordsContainer provisioned successfully.");
    }
}
