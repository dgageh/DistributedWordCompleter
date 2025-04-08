
using IngestorService.Services;

namespace IngestorService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<WordIngestorService>();
            builder.Services.AddHttpClient<IPrefixTreeClient, PrefixTreeClient>();
            builder.Services.AddSingleton<IWordsDbService, WordsDbService>(provider =>
            {
                string cosmosEndpoint = builder.Configuration["Cosmos:Endpoint"];
                string cosmosKey = builder.Configuration["Cosmos:Key"];
                return new WordsDbService(cosmosEndpoint, cosmosKey);
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
