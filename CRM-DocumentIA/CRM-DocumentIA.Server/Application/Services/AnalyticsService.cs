using CRM_DocumentIA.Server.Application.DTOs.Analytics;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Domain.Interfaces.Repositories;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class AnalyticsService 
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IInsightsHistoRepository _insightsRepository;

        public AnalyticsService(
            IUsuarioRepository usuarioRepository,
            IChatRepository chatRepository,
            IInsightsHistoRepository insightsRepository)
        {
            _usuarioRepository = usuarioRepository;
            _chatRepository = chatRepository;
            _insightsRepository = insightsRepository;
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
    }
}
