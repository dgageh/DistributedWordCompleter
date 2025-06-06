using TrieLibrary;

namespace PrefixTreeServiceB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Configure Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add custom services like TrieLibrary dependency
            builder.Services.AddSingleton<IConcurrentTrie, ConcurrentTrie>();
            builder.Services.AddSwaggerGen(options =>
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
            var app = builder.Build();

            app.UseSwagger(); // Enable the Swagger middleware


            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PrefixTreeServiceB API V1");
                c.RoutePrefix = string.Empty; // Set the Swagger UI to be the default page
            });


            // If you are not using HTTPS, you can comment out or remove the UseHttpsRedirection
            app.UseHttpsRedirection();

            app.UseAuthorization(); // Add authorization middleware if needed

            // Map controllers
            app.MapControllers();

            // Run the application
            app.Run();
        }
    }
}
