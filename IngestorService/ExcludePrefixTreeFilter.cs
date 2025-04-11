using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class ExcludePrefixTreeFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Remove schemas related to PrefixTree
        var schemasToRemove = context.SchemaRepository.Schemas
            .Where(schema => schema.Key.Contains("PrefixTree"))
            .Select(schema => schema.Key)
            .ToList();

        foreach (var schema in schemasToRemove)
        {
            context.SchemaRepository.Schemas.Remove(schema);
        }
    }
}
