using Microsoft.Azure.Cosmos;
using System.Security;
using Container = Microsoft.Azure.Cosmos.Container;

class Program
{
    private static readonly string EndpointUri = "https://localhost:8081/";
    private static readonly string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
    private static readonly string DatabaseId = "WordsDB";
    private static readonly string ContainerId = "WordsContainer";

static async Task Main()
    {

        CosmosClientOptions options = new CosmosClientOptions
        {
            HttpClientFactory = () =>
            {
                HttpClientHandler handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                return new HttpClient(handler);
            },
            ConnectionMode = ConnectionMode.Gateway,
            ApplicationPreferredRegions = new List<string> { "West US" }
        };

        var cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, options);

        var databases = cosmosClient.GetDatabaseQueryIterator<DatabaseProperties>();
        while (databases.HasMoreResults)
        {
            foreach (var db in await databases.ReadNextAsync())
            {
                Console.WriteLine($"Found DB: {db.Id}");
            }
        }

        //await CreateDatabaseAndContainerAsync(cosmosClient);

        //var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

        //await AddWordsAsync(container);
        //await QueryWordsAsync(container);
    }



    private static async Task CreateDatabaseAndContainerAsync(CosmosClient client)
    {
        // Create Database if not exists
        Database database = await client.CreateDatabaseIfNotExistsAsync(DatabaseId);
        Console.WriteLine($"Created Database: {database.Id}");

        // Define Partition Key (First letter of word)
        ContainerProperties containerProperties = new(ContainerId, "/word[0]");

        // Create Container if not exists
        Container container = await database.CreateContainerIfNotExistsAsync(containerProperties);
        Console.WriteLine($"Created Container: {container.Id}");
    }

    private static async Task AddWordsAsync(Container container)
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "words.txt");

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        // Read all lines asynchronously
        string[] words = await File.ReadAllLinesAsync(filePath);

        foreach (string word in words)
        {
            // Trim whitespace and check if word is valid
            if (!string.IsNullOrWhiteSpace(word))
            {
                await AddWordAsync(container, word.Trim());
            }
        }

        Console.WriteLine("Finished inserting words.");
    }

    private static async Task AddWordAsync(Container container, string word)
    {
        try
        {
            var wordItem = new
            {
                id = $"word-{word}", // Unique ID
                word = word,
                createdAt = DateTime.UtcNow
            };

            await container.CreateItemAsync(wordItem);
            Console.WriteLine($"Inserted word: {word}");
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            Console.WriteLine($"Word '{word}' already exists. Skipping insertion.");
        }
    }

    private static async Task QueryWordsAsync(Container container)
    {
        var queryDefinition = new QueryDefinition("SELECT * FROM c");
        using FeedIterator<dynamic> feedIterator = container.GetItemQueryIterator<dynamic>(queryDefinition);

        while (feedIterator.HasMoreResults)
        {
            FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
            foreach (var item in response)
            {
                Console.WriteLine($"Word: {item.word}, Created At: {item.createdAt}");
            }
        }
    }
}
