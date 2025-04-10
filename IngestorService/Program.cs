using IngestorService.Services;
using Microsoft.Extensions.DependencyInjection;
using PrefixTreeServiceA.Services;
using TrieLibrary;

namespace IngestorService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();

            // Configure Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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
                var cosmosEndpoint = builder.Configuration["Cosmos:Endpoint"];
                var cosmosKey = builder.Configuration["Cosmos:Key"];
                if (string.IsNullOrEmpty(cosmosKey) || string.IsNullOrEmpty(cosmosEndpoint))
                {
                    throw new InvalidOperationException("The configuration value for Cosmos:Key or Cosmos:Endpoint are null");
                }

                return new WordsDbService(cosmosEndpoint, cosmosKey);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            app.UseSwagger(); // Enable Swagger middleware
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "IngestorService API V1");
                c.RoutePrefix = string.Empty; // Set Swagger UI as the default page
            });

            // If you are not using HTTPS, you can comment out or remove the UseHttpsRedirection
            app.UseHttpsRedirection();

            app.UseAuthorization(); // Authorization middleware (if needed)

            // Map controllers
            app.MapControllers();

            // Run the application
            app.Run();
        }
    }
}
