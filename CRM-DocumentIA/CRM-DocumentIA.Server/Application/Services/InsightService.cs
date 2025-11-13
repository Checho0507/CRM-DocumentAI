using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class InsightService
    {
        private readonly IInsightRepository _insightRepository;

        public InsightService(IInsightRepository insightRepository)
        {
            _insightRepository = insightRepository;
        }

        public async Task<IEnumerable<Insight>> ObtenerTodosAsync()
            => await _insightRepository.ObtenerTodosAsync();

        public async Task<Insight?> ObtenerPorIdAsync(int id)
            => await _insightRepository.ObtenerPorIdAsync(id);

        public async Task<IEnumerable<Insight>> ObtenerPorDocumentoIdAsync(int documentoId)
            => await _insightRepository.ObtenerPorDocumentoIdAsync(documentoId);

        public async Task<IEnumerable<Insight>> ObtenerPorTipoAsync(string tipoInsight)
            => await _insightRepository.ObtenerPorTipoAsync(tipoInsight);

        public async Task AgregarAsync(Insight insight)
            => await _insightRepository.AgregarAsync(insight);

        public async Task ActualizarAsync(Insight insight)
            => await _insightRepository.ActualizarAsync(insight);

        public async Task EliminarAsync(int id)
            => await _insightRepository.EliminarAsync(id);
    }
}