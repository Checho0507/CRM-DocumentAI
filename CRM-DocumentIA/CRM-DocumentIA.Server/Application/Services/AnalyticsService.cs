using CRM_DocumentIA.Server.Application.DTOs.Analytics;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Domain.Interfaces.Repositories;
using CRM_DocumentIA.Server.Infrastructure.Repositories;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class AnalyticsService 
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IInsightsHistoRepository _insightsRepository;
        private readonly IDocumentoRepository _documentoRepository;

        public AnalyticsService(
            IUsuarioRepository usuarioRepository,
            IChatRepository chatRepository,
            IInsightsHistoRepository insightsRepository,
            IDocumentoRepository documentoRepository)
        {
            _usuarioRepository = usuarioRepository;
            _chatRepository = chatRepository;
            _insightsRepository = insightsRepository;
            _documentoRepository = documentoRepository;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var totalUsuarios = await _usuarioRepository.CountAsync();
            var totalChats = await _chatRepository.CountAsync();
            var totalConsultas = await _insightsRepository.CountAsync();

            var usuariosActivos = await _insightsRepository.CountDistinctUsersLastDaysAsync(30);

            return new DashboardSummaryDto
            {
                TotalUsuarios = totalUsuarios,
                TotalChats = totalChats,
                TotalConsultas = totalConsultas,
                UsuariosActivosUltimos30Dias = usuariosActivos
            };
        }

        public async Task<IEnumerable<ConsultasPorDiaDto>> GetConsultasPorDiaAsync()
        {
            var consultas = await _insightsRepository.GetConsultasGroupedByDayAsync();

            return consultas.Select(c => new ConsultasPorDiaDto
            {
                Fecha = c.Fecha,
                Cantidad = c.Cantidad
            });
        }

        public async Task<IEnumerable<TopUsuariosDto>> GetTopUsuariosAsync()
        {
            var data = await _insightsRepository.GetTopUsersAsync(10);

            return data.Select(x => new TopUsuariosDto
            {
                UserId = x.UserId,
                Nombre = x.Nombre,
                TotalConsultas = x.Total
            });
        }

        public async Task<IEnumerable<DocumentosPorEstadoDto>> GetDocumentosPorEstadoAsync()
        {
            var data = await _documentoRepository.GetDocumentosAgrupadosPorEstadoAsync();

            return data.Select(d => new DocumentosPorEstadoDto
            {
                Estado = d.Estado,
                Cantidad = d.Cantidad
            });
        }

        public async Task<IEnumerable<DocumentosPorTipoDto>> GetDocumentosPorTipoAsync()
        {
            var data = await _documentoRepository.GetDocumentosAgrupadosPorTipoAsync();

            return data.Select(d => new DocumentosPorTipoDto
            {
                Tipo = d.Tipo,
                Cantidad = d.Cantidad
            });
        }
    }
}
