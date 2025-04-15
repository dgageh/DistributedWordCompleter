using Azure.Identity;
using IngestorService.Services;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
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
            var prefixTreeServiceUrl = Environment.GetEnvironmentVariable("PREFIX-TREE-SERVICE") ?? "http://localhost:8000";
            var cosmosEndpoint = Environment.GetEnvironmentVariable("COSMOS-ENDPOINT");
            var cosmosKey = Environment.GetEnvironmentVariable("COSMOS-KEY");


            // Add services to the container
            builder.Services.AddControllers();

            // Configure Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.FullName); // Ensure unique schema IDs
                options.DocumentFilter<ExcludePrefixTreeFilter>();
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            // Add application services
            builder.Services.AddSingleton<IWordsDbService, WordsDbService>();
            builder.Services.AddSingleton<IPrefixTreeClient, PrefixTreeClient>();
            builder.Services.AddSingleton<WordIngestorService>();
            builder.Services.AddHttpClient<IPrefixTreeClient, PrefixTreeClient>(httpClient =>
            {
                if (string.IsNullOrEmpty(prefixTreeServiceUrl))
                {
                    throw new InvalidOperationException("The configuration value for 'PrefixTreeService:BaseUrl' is missing.");
                }
                httpClient.BaseAddress = new Uri(prefixTreeServiceUrl);
            });

            // Configure Cosmos DB service with dependency injection
            builder.Services.AddSingleton<IWordsDbService, WordsDbService>(provider =>
            {
                if (string.IsNullOrEmpty(cosmosKey) || string.IsNullOrEmpty(cosmosEndpoint))
                {
                    throw new InvalidOperationException("The configuration value for Cosmos:Key or Cosmos:Endpoint are null");
                }

                return new WordsDbService(cosmosEndpoint, cosmosKey);
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
