using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Application.DTOs.Insight;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class InsightService
    {
        private readonly IInsightRepository _insightRepository;
        private readonly IDocumentoRepository _documentoRepository;
        private readonly ProcesoIAService _procesoIAService;

        public InsightService(
            IInsightRepository insightRepository,
            IDocumentoRepository documentoRepository,
            ProcesoIAService procesoIAService)
        {
            _insightRepository = insightRepository;
            _documentoRepository = documentoRepository;
            _procesoIAService = procesoIAService;
        }

        // Métodos básicos del repositorio
        public Task<IEnumerable<Insight>> ObtenerTodosAsync()
            => _insightRepository.ObtenerTodosAsync();

        public Task<Insight?> ObtenerPorIdAsync(int id)
            => _insightRepository.ObtenerPorIdAsync(id);

        public Task<IEnumerable<Insight>> ObtenerPorDocumentoIdAsync(int documentoId)
            => _insightRepository.ObtenerPorDocumentoIdAsync(documentoId);

        public Task<IEnumerable<Insight>> ObtenerPorProcesoIAIdAsync(int procesoIAId)
            => _insightRepository.ObtenerPorProcesoIAIdAsync(procesoIAId);

        public Task<IEnumerable<Insight>> ObtenerPorTipoAsync(string tipo)
            => _insightRepository.ObtenerPorTipoAsync(tipo);

        // Métodos de negocio
        public async Task<Insight> CrearInsightAsync(InsightCreateDto dto)
        {
            var insight = new Insight
            {
                DocumentoId = dto.DocumentoId,
                ProcesoIAId = dto.ProcesoIAId,
                TipoInsight = dto.TipoInsight,
                Contenido = dto.Contenido,
                Confianza = dto.Confianza,
                FechaGeneracion = DateTime.UtcNow
            };

            await _insightRepository.AgregarAsync(insight);
            return insight;
        }

        public async Task<Insight?> ActualizarInsightAsync(int id, InsightUpdateDto dto)
        {
            var insight = await _insightRepository.ObtenerPorIdAsync(id);
            if (insight == null) return null;

            if (!string.IsNullOrEmpty(dto.TipoInsight))
                insight.TipoInsight = dto.TipoInsight;

            if (!string.IsNullOrEmpty(dto.Contenido))
                insight.Contenido = dto.Contenido;

            if (dto.Confianza.HasValue)
                insight.Confianza = dto.Confianza.Value;

            await _insightRepository.ActualizarAsync(insight);
            return insight;
        }

        public async Task<bool> EliminarInsightAsync(int id)
        {
            var insight = await _insightRepository.ObtenerPorIdAsync(id);
            if (insight == null) return false;

            await _insightRepository.EliminarAsync(insight);
            return true;
        }

        // Método para generar insights desde RAG
        public async Task<Insight> GenerarInsightDesdeRAGAsync(int documentoId, string pregunta, string tipoInsight = "analisis")
        {
            // 1. Verificar que el documento existe y está procesado
            var documento = await _documentoRepository.ObtenerPorIdAsync(documentoId);
            if (documento == null)
                throw new ArgumentException($"Documento con ID {documentoId} no encontrado");

            if (!documento.Procesado || documento.EstadoProcesamiento != "completado")
                throw new InvalidOperationException("El documento debe estar procesado para generar insights");

            // 2. Llamar al servicio RAG a través de ProcesoIAService
            var resultadoRAG = await _procesoIAService.GenerarInsightAsync(documentoId, pregunta);

            // 3. Crear el insight con la respuesta del RAG
            var insight = new Insight
            {
                DocumentoId = documentoId,
                ProcesoIAId = resultadoRAG.ProcesoIAId, // El servicio puede devolver el ID del proceso
                TipoInsight = tipoInsight,
                Contenido = resultadoRAG.Respuesta,
                Confianza = resultadoRAG.Confianza,
                FechaGeneracion = DateTime.UtcNow
            };

            await _insightRepository.AgregarAsync(insight);
            return insight;
        }
    }
}