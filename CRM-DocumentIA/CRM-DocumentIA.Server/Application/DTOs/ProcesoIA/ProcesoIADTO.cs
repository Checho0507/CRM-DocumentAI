using System.Text.Json;

namespace CRM_DocumentIA.Server.Application.DTOs.Documento
{
    public class ProcesamientoIADto
    {
        public bool Exito { get; set; }
        public int NumeroImagenes { get; set; }
        public string Resumen { get; set; } = string.Empty;
        public string ContenidoExtraido { get; set; } = string.Empty;
        public string? Error { get; set; }
        public Dictionary<string, object>? MetadataAdicional { get; set; }
        
        // Método para serializar metadata
        public string? MetadataAdicionalJson 
        { 
            get 
            {
                if (MetadataAdicional == null || MetadataAdicional.Count == 0)
                    return null;
                    
                try
                {
                    return JsonSerializer.Serialize(MetadataAdicional, new JsonSerializerOptions
                    {
                        WriteIndented = false
                    });
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}