using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class ProcesoIAService
    {
        private readonly IProcesoIARepository _procesoIARepository;

        public ProcesoIAService(IProcesoIARepository procesoIARepository)
        {
            _procesoIARepository = procesoIARepository;
        }

        public Task<IEnumerable<ProcesoIA>> ObtenerTodosAsync() => _procesoIARepository.ObtenerTodosAsync();
        public Task<ProcesoIA?> ObtenerPorIdAsync(int id) => _procesoIARepository.ObtenerPorIdAsync(id);
        public Task AgregarAsync(ProcesoIA procesoIA) => _procesoIARepository.AgregarAsync(procesoIA);
        public Task ActualizarAsync(ProcesoIA procesoIA) => _procesoIARepository.ActualizarAsync(procesoIA);
        public Task EliminarAsync(int id) => _procesoIARepository.EliminarAsync(id);
    }
}
