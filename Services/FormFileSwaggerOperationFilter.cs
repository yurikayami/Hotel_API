using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Hotel_API.Services
{
    /// <summary>
    /// Swagger operation filter để handle IFormFile parameters
    /// </summary>
    public class FormFileSwaggerOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var formFileParams = context.ApiDescription.ActionDescriptor.Parameters
                .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFileCollection))
                .Select(p => p.Name)
                .ToList();

            if (formFileParams.Count == 0)
                return;

            // Remove parameters that are IFormFile
            foreach (var paramName in formFileParams)
            {
                var paramToRemove = operation.Parameters.FirstOrDefault(p => p.Name == paramName);
                if (paramToRemove != null)
                {
                    operation.Parameters.Remove(paramToRemove);
                }
            }

            // Add form file parameter
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["image"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary",
                                    Description = "File ảnh"
                                },
                                ["userId"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Description = "ID của user"
                                },
                                ["mealType"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Description = "Loại bữa ăn (breakfast, lunch, dinner, snack)"
                                }
                            },
                            Required = new HashSet<string> { "image", "userId" }
                        }
                    }
                }
            };
        }
    }
}
