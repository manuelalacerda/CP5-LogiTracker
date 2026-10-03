using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LogiTracker.API.Swagger;

public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var d in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(d.GroupName, new OpenApiInfo
            {
                Title = "LogiTracker API",
                Version = d.ApiVersion.ToString(),
                Description = d.IsDeprecated
                    ? "⚠️ Esta versão está DEPRECADA. Migre para a v2.0 (listagem paginada)."
                    : "Versão atual. Listagem de entregas paginada."
            });
        }
    }
}