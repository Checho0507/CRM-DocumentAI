// Application/Services/InsightService.cs

using CRM_DocumentIA.Server.Application.DTOs.Insight;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;

namespace CRM_DocumentIA.Server.Application.Services
{
    // Se elimina la herencia de IServicioInsight y se cambia el nombre a InsightService
    public class InsightService
    {
        // Se cambia el nombre de la dependencia a IInsightRepository
        private readonly IInsightRepository _insightRepository;
        
        // Se cambia el nombre del parámetro del constructor
        public InsightService(IInsightRepository insightRepository)
        {
            _insightRepository = insightRepository;
        }

        public async Task<List<InsightDTO>> ObtenerInsightsPorDocumentoAsync(int documentoId)
        {
            var insights = await _insightRepository.ObtenerPorDocumentoIdAsync(documentoId);

            // Mapeo de Entidades a DTOs
            return insights.Select(i => new InsightDTO 
            {
                Id = i.Id,
                Tipo = i.Tipo,
                Contenido = i.Contenido,
                GeneradoEn = i.GeneradoEn
            }).ToList();
        }

        public async Task<InsightDTO> GenerarYGuardarInsightAsync(int documentoId, string tipoAnalisis)
        {
            // *** 1. Lógica de negocio para interactuar con la IA (PENDIENTE de implementación) ***
            // En una aplicación real, aquí va el cliente HTTP que llama al microservicio de RAG/IA.
            string contenidoGenerado = $"[SIMULACIÓN DE IA: {tipoAnalisis}] Resumen clave del documento ID {documentoId}.";
            
            // 2. Creación de la Entidad
            var nuevoInsight = new Insight
            {
                DocumentoId = documentoId,
                Tipo = tipoAnalisis,
                Contenido = contenidoGenerado,
                ClienteId = null, 
                GeneradoEn = DateTime.UtcNow
            };

            // 3. Guardar en la base de datos
            await _insightRepository.AgregarAsync(nuevoInsight);

            // 4. Devolver DTO
            return new InsightDTO
            {
                Id = nuevoInsight.Id,
                Tipo = nuevoInsight.Tipo,
                Contenido = nuevoInsight.Contenido,
                GeneradoEn = nuevoInsight.GeneradoEn
            };
        }
    }
}