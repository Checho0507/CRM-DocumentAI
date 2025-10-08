using CRM_DocumentIA.Server.Domain.Entities;

namespace CRM_DocumentIA.Server.Domain.Entities
{
    public class Insight
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty; // e.g., "Resumen", "AcciónClave", "Sentimiento"
        public string Contenido { get; set; } = string.Empty; // El resultado del análisis RAG/NLP

        public DateTime GeneradoEn { get; set; } = DateTime.UtcNow;

        // Relaciones con otras entidades
        public int DocumentoId { get; set; }
        public Documento Documento { get; set; } = null!; // Navegación

        // Opcional: Relación a Cliente si el Insight es a nivel de cliente
        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; } // Opcional
    }
}