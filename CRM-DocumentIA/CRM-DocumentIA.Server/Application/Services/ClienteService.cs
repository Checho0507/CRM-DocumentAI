using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class ClienteService
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public Task<IEnumerable<Cliente>> ObtenerTodosAsync() => _clienteRepository.ObtenerTodosAsync();
        public Task<Cliente?> ObtenerPorIdAsync(int id) => _clienteRepository.ObtenerPorIdAsync(id);
        public Task AgregarAsync(Cliente cliente) => _clienteRepository.AgregarAsync(cliente);
        public Task ActualizarAsync(Cliente cliente) => _clienteRepository.ActualizarAsync(cliente);
        public Task EliminarAsync(int id) => _clienteRepository.EliminarAsync(id);
    }
}
