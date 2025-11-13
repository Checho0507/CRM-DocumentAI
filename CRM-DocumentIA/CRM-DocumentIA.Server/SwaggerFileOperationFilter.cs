using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace CRM_DocumentIA.Server
{
    public class SwaggerFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var parameters = context.MethodInfo.GetParameters();
            var hasFormFile = parameters.Any(p => 
                p.ParameterType == typeof(IFormFile) || 
                p.ParameterType == typeof(IFormFileCollection));

            if (hasFormFile)
            {
                var properties = new Dictionary<string, OpenApiSchema>();
                var required = new HashSet<string>();

                foreach (var param in parameters)
                {
                    if (param.ParameterType == typeof(IFormFile))
                    {
                        properties[param.Name ?? "archivo"] = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary",
                            Description = "Archivo a subir"
                        };
                        required.Add(param.Name ?? "archivo");
                    }
                    else if (param.ParameterType == typeof(int))
                    {
                        properties[param.Name ?? "usuarioId"] = new OpenApiSchema
                        {
                            Type = "integer",
                            Format = "int32",
                            Description = "ID del usuario"
                        };
                        required.Add(param.Name ?? "usuarioId");
                    }
                }

                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = properties,
                                Required = required
                            }
                        }
                    }
                };

                // Limpiar parámetros existentes
                operation.Parameters = operation.Parameters?
                    .Where(p => !parameters.Any(fp => fp.Name == p.Name && 
                        fp.ParameterType == typeof(IFormFile)))
                    .ToList();
            }
        }
    }
}