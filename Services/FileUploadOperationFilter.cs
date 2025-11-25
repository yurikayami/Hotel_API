using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Hotel_API.Services
{
    /// <summary>
    /// Custom Swagger operation filter for handling IFormFile parameters
    /// </summary>
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileUploadMime = "multipart/form-data";

            if (operation.RequestBody == null)
                return;

            if (!operation.RequestBody.Content.ContainsKey(fileUploadMime))
                return;

            var fileParams = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile) ||
                           (p.ParameterType.IsGenericType &&
                            p.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                            p.ParameterType.GenericTypeArguments.FirstOrDefault() == typeof(IFormFile)));

            foreach (var fileParam in fileParams)
            {
                if (operation.RequestBody.Content[fileUploadMime].Schema.Properties == null)
                    operation.RequestBody.Content[fileUploadMime].Schema.Properties = new Dictionary<string, OpenApiSchema>();

                operation.RequestBody.Content[fileUploadMime].Schema.Properties[fileParam.Name] = new OpenApiSchema()
                {
                    Type = "string",
                    Format = "binary"
                };
            }
        }
    }
}
