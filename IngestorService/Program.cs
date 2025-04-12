using Azure.Identity;
using IngestorService.Services;
using Microsoft.Extensions.DependencyInjection;
using TrieLibrary;

namespace IngestorService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Read the designated region from an environment variable (or fallback to a default value)
            var region = Environment.GetEnvironmentVariable("REGION") ?? "local";

            // Optionally, load a region-specific settings file
            builder.Configuration.AddJsonFile($"appsettings.{region}.json", optional: false, reloadOnChange: true);

            // Add Azure Key Vault integration
            var keyVaultName = builder.Configuration["KeyVault:Name"];
            if (string.IsNullOrEmpty(keyVaultName))
            {
                throw new InvalidOperationException("KeyVault:Name is not configured in appsettings.json or environment variables.");
            }

            var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
            builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());

            // Add services to the container
            builder.Services.AddControllers();

            // Configure Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();

            // Add application services
            builder.Services.AddSingleton<IWordsDbService, WordsDbService>();

            builder.Services.AddSingleton<IPrefixTreeClient, PrefixTreeClient>();
            builder.Services.AddSingleton<WordIngestorService>();
            builder.Services.AddHttpClient<IPrefixTreeClient, PrefixTreeClient>(httpClient =>
            {
                var baseUrl = builder.Configuration["PrefixTreeService:BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new InvalidOperationException("The configuration value for 'PrefixTreeService:BaseUrl' is missing.");
                }
                httpClient.BaseAddress = new Uri(baseUrl);
            });

            // Configure Cosmos DB service with dependency injection
            builder.Services.AddSingleton<IWordsDbService, WordsDbService>(provider =>
            {
                var cosmosEndpoint = builder.Configuration["CosmosEndpoint"];
                var cosmosKey = builder.Configuration["CosmosKey"];
                if (string.IsNullOrEmpty(cosmosKey) || string.IsNullOrEmpty(cosmosEndpoint))
                {
                    throw new InvalidOperationException("The configuration value for Cosmos:Key or Cosmos:Endpoint are null");
                }

                return new WordsDbService(cosmosEndpoint, cosmosKey);
            });

            builder.Services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.FullName); // Ensure unique schema IDs
                options.DocumentFilter<ExcludePrefixTreeFilter>();
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "IngestorService API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
