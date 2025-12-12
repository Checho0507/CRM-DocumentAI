using CRM_DocumentIA.Server.Application.DTOs.Documento;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class DocumentoCompartidoService
    {
        private readonly IDocumentoCompartidoRepository _documentoCompartidoRepository;
        private readonly IDocumentoRepository _documentoRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<DocumentoCompartidoService> _logger;

        public DocumentoCompartidoService(
            IDocumentoCompartidoRepository documentoCompartidoRepository,
            IDocumentoRepository documentoRepository,
            IUsuarioRepository usuarioRepository,
            ILogger<DocumentoCompartidoService> logger)
        {
            _documentoCompartidoRepository = documentoCompartidoRepository;
            _documentoRepository = documentoRepository;
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }

        public async Task<DocumentoCompartido> CompartirDocumentoAsync(CompartirDocumentoDTO dto, int usuarioPropietarioId)
        {
            try
            {
                _logger.LogInformation($"🔄 Iniciando compartir documento {dto.DocumentoId} con usuario {dto.UsuarioCompartidoId}");

                // 1. Verificar que el documento existe
                var documento = await _documentoRepository.ObtenerPorIdAsync(dto.DocumentoId);
                if (documento == null)
                {
                    _logger.LogWarning($"❌ Documento {dto.DocumentoId} no encontrado");
                    throw new KeyNotFoundException($"Documento {dto.DocumentoId} no encontrado");
                }

                // 2. Verificar que el usuario es propietario del documento
                if (documento.UsuarioId != usuarioPropietarioId)
                {
                    _logger.LogWarning($"❌ Usuario {usuarioPropietarioId} no es propietario del documento {dto.DocumentoId}");
                    throw new UnauthorizedAccessException("No tienes permisos para compartir este documento");
                }

                // 3. Verificar que el usuario a compartir existe
                var usuarioCompartido = await _usuarioRepository.ObtenerPorIdAsync(dto.UsuarioCompartidoId);
                if (usuarioCompartido == null)
                {
                    _logger.LogWarning($"❌ Usuario a compartir {dto.UsuarioCompartidoId} no encontrado");
                    throw new KeyNotFoundException($"Usuario {dto.UsuarioCompartidoId} no encontrado");
                }

                // 4. Verificar que no se comparta consigo mismo
                if (dto.UsuarioCompartidoId == usuarioPropietarioId)
                {
                    _logger.LogWarning($"❌ Intento de compartir documento consigo mismo");
                    throw new InvalidOperationException("No puedes compartir un documento contigo mismo");
                }

                // 5. Verificar que el documento no esté ya compartido con ese usuario
                bool yaCompartido = await _documentoCompartidoRepository.ExisteCompartidoAsync(dto.DocumentoId, dto.UsuarioCompartidoId);
                if (yaCompartido)
                {
                    _logger.LogWarning($"❌ Documento {dto.DocumentoId} ya está compartido con usuario {dto.UsuarioCompartidoId}");
                    throw new InvalidOperationException("El documento ya está compartido con este usuario");
                }

                // 6. Crear el registro de documento compartido
                var documentoCompartido = new DocumentoCompartido
                {
                    DocumentoId = dto.DocumentoId,
                    UsuarioPropietarioId = usuarioPropietarioId,
                    UsuarioCompartidoId = dto.UsuarioCompartidoId,
                    Permiso = dto.Permiso?.ToLower() ?? "lectura",
                    Mensaje = dto.Mensaje,
                    FechaCompartido = DateTime.UtcNow
                };

                // 7. Guardar en la base de datos
                var resultado = await _documentoCompartidoRepository.AgregarAsync(documentoCompartido);
                
                _logger.LogInformation($"✅ Documento {dto.DocumentoId} compartido exitosamente con usuario {dto.UsuarioCompartidoId}");
                
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al compartir documento {dto.DocumentoId}");
                throw;
            }
        }

        public async Task<IEnumerable<DocumentoCompartido>> ObtenerDocumentosCompartidosConmigoAsync(int usuarioId)
        {
            try
            {
                _logger.LogInformation($"🔄 Obteniendo documentos compartidos con usuario {usuarioId}");

                // 1. Obtener documentos compartidos con este usuario
                var documentosCompartidos = await _documentoCompartidoRepository
                    .ObtenerDocumentosCompartidosConUsuarioAsync(usuarioId);

                _logger.LogInformation($"📊 Encontrados {documentosCompartidos.Count()} documentos compartidos con usuario {usuarioId}");

                // 2. Cargar datos relacionados para cada documento compartido
                foreach (var dc in documentosCompartidos)
                {
                    try
                    {
                        // Cargar información del documento
                        dc.Documento = await _documentoRepository.ObtenerPorIdAsync(dc.DocumentoId);
                        
                        // Cargar información del usuario propietario
                        dc.UsuarioPropietario = await _usuarioRepository.ObtenerPorIdAsync(dc.UsuarioPropietarioId);
                        
                        // Cargar información del usuario compartido (que es el actual, pero por consistencia)
                        dc.UsuarioCompartido = await _usuarioRepository.ObtenerPorIdAsync(dc.UsuarioCompartidoId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"⚠️ Error al cargar datos relacionados para DocumentoCompartido {dc.Id}");
                        // Continuar con el siguiente
                    }
                }

                return documentosCompartidos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener documentos compartidos con usuario {usuarioId}");
                throw;
            }
        }

        public async Task<IEnumerable<DocumentoCompartido>> ObtenerDocumentosQueHeCompartidoAsync(int usuarioId)
        {
            try
            {
                _logger.LogInformation($"🔄 Obteniendo documentos compartidos por usuario {usuarioId}");

                // 1. Obtener documentos compartidos por este usuario
                var documentosCompartidos = await _documentoCompartidoRepository
                    .ObtenerDocumentosCompartidosPorPropietarioAsync(usuarioId);

                _logger.LogInformation($"📊 Usuario {usuarioId} ha compartido {documentosCompartidos.Count()} documentos");

                // 2. Cargar datos relacionados para cada documento compartido
                foreach (var dc in documentosCompartidos)
                {
                    try
                    {
                        // Cargar información del documento
                        dc.Documento = await _documentoRepository.ObtenerPorIdAsync(dc.DocumentoId);
                        
                        // Cargar información del usuario compartido
                        dc.UsuarioCompartido = await _usuarioRepository.ObtenerPorIdAsync(dc.UsuarioCompartidoId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"⚠️ Error al cargar datos relacionados para DocumentoCompartido {dc.Id}");
                        // Continuar con el siguiente
                    }
                }

                return documentosCompartidos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener documentos compartidos por usuario {usuarioId}");
                throw;
            }
        }

        public async Task<IEnumerable<Usuario>> ObtenerUsuariosCompartidosAsync(int documentoId, int usuarioPropietarioId)
        {
            try
            {
                _logger.LogInformation($"🔄 Obteniendo usuarios con los que se compartió documento {documentoId}");

                // 1. Verificar que el documento existe y pertenece al usuario
                var documento = await _documentoRepository.ObtenerPorIdAsync(documentoId);
                if (documento == null)
                {
                    _logger.LogWarning($"❌ Documento {documentoId} no encontrado");
                    throw new KeyNotFoundException($"Documento {documentoId} no encontrado");
                }

                if (documento.UsuarioId != usuarioPropietarioId)
                {
                    _logger.LogWarning($"❌ Usuario {usuarioPropietarioId} no es propietario del documento {documentoId}");
                    throw new UnauthorizedAccessException("No tienes permisos para ver los usuarios compartidos de este documento");
                }

                // 2. Obtener registros de compartidos para este documento
                var compartidos = await _documentoCompartidoRepository
                    .ObtenerCompartidosPorDocumentoAsync(documentoId);

                _logger.LogInformation($"📊 Documento {documentoId} compartido con {compartidos.Count()} usuarios");

                // 3. Obtener los usuarios a partir de los registros
                var usuarios = new List<Usuario>();
                foreach (var compartido in compartidos)
                {
                    try
                    {
                        var usuario = await _usuarioRepository.ObtenerPorIdAsync(compartido.UsuarioCompartidoId);
                        if (usuario != null)
                        {
                            usuarios.Add(usuario);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"⚠️ Error al cargar usuario {compartido.UsuarioCompartidoId}");
                    }
                }

                return usuarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener usuarios compartidos para documento {documentoId}");
                throw;
            }
        }

        public async Task<bool> DejarDeCompartirAsync(int compartidoId, int usuarioPropietarioId)
        {
            try
            {
                _logger.LogInformation($"🔄 Intentando eliminar compartido {compartidoId} por usuario {usuarioPropietarioId}");

                // 1. Verificar que el registro de compartido existe
                var compartido = await _documentoCompartidoRepository.ObtenerPorIdAsync(compartidoId);
                if (compartido == null)
                {
                    _logger.LogWarning($"❌ Compartido {compartidoId} no encontrado");
                    return false;
                }

                // 2. Verificar que el usuario es el propietario del documento
                if (compartido.UsuarioPropietarioId != usuarioPropietarioId)
                {
                    _logger.LogWarning($"❌ Usuario {usuarioPropietarioId} no es propietario del compartido {compartidoId}");
                    throw new UnauthorizedAccessException("No tienes permisos para eliminar este compartido");
                }

                // 3. Eliminar el registro
                await _documentoCompartidoRepository.EliminarAsync(compartidoId);
                
                _logger.LogInformation($"✅ Compartido {compartidoId} eliminado exitosamente");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al eliminar compartido {compartidoId}");
                throw;
            }
        }

        public async Task<bool> VerificarPermisoAsync(int documentoId, int usuarioId)
        {
            try
            {
                _logger.LogInformation($"🔄 Verificando permiso para documento {documentoId} y usuario {usuarioId}");

                // 1. Verificar si el usuario es propietario del documento
                var documento = await _documentoRepository.ObtenerPorIdAsync(documentoId);
                if (documento != null && documento.UsuarioId == usuarioId)
                {
                    _logger.LogInformation($"✅ Usuario {usuarioId} es propietario del documento {documentoId}");
                    return true;
                }

                // 2. Verificar si el documento está compartido con el usuario
                var tieneAccesoCompartido = await _documentoCompartidoRepository.ExisteCompartidoAsync(documentoId, usuarioId);
                
                if (tieneAccesoCompartido)
                {
                    _logger.LogInformation($"✅ Usuario {usuarioId} tiene acceso compartido al documento {documentoId}");
                }
                else
                {
                    _logger.LogInformation($"❌ Usuario {usuarioId} NO tiene acceso al documento {documentoId}");
                }
                
                return tieneAccesoCompartido;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al verificar permiso para documento {documentoId}");
                // En caso de error, asumimos que no tiene permiso
                return false;
            }
        }

        public async Task<IEnumerable<DocumentoCompartido>> ObtenerTodosLosCompartidosAsync()
        {
            try
            {
                _logger.LogInformation("🔄 Obteniendo todos los documentos compartidos");

                // 1. Obtener todos los registros de documentos compartidos
                var todosCompartidos = await _documentoCompartidoRepository.ObtenerTodosAsync();

                _logger.LogInformation($"📊 Total de documentos compartidos en el sistema: {todosCompartidos.Count()}");

                // 2. Cargar datos relacionados para cada registro
                foreach (var compartido in todosCompartidos)
                {
                    try
                    {
                        // Cargar información del documento
                        compartido.Documento = await _documentoRepository.ObtenerPorIdAsync(compartido.DocumentoId);
                        
                        // Cargar información del usuario propietario
                        compartido.UsuarioPropietario = await _usuarioRepository.ObtenerPorIdAsync(compartido.UsuarioPropietarioId);
                        
                        // Cargar información del usuario compartido
                        compartido.UsuarioCompartido = await _usuarioRepository.ObtenerPorIdAsync(compartido.UsuarioCompartidoId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"⚠️ Error al cargar datos relacionados para DocumentoCompartido {compartido.Id}");
                    }
                }

                return todosCompartidos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener todos los documentos compartidos");
                throw;
            }
        }

        // Método adicional útil: Obtener permisos específicos de un documento compartido
        public async Task<string?> ObtenerPermisoCompartidoAsync(int documentoId, int usuarioId)
        {
            try
            {
                // Buscar el registro específico de documento compartido
                var compartidos = await _documentoCompartidoRepository.ObtenerTodosAsync();
                var compartido = compartidos.FirstOrDefault(dc => 
                    dc.DocumentoId == documentoId && dc.UsuarioCompartidoId == usuarioId);
                
                return compartido?.Permiso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener permiso compartido para documento {documentoId}");
                return null;
            }
        }

        // Método adicional: Verificar si un usuario puede editar un documento
        public async Task<bool> PuedeEditarDocumentoAsync(int documentoId, int usuarioId)
        {
            try
            {
                // Primero verificar si es propietario
                var documento = await _documentoRepository.ObtenerPorIdAsync(documentoId);
                if (documento != null && documento.UsuarioId == usuarioId)
                {
                    return true; // El propietario siempre puede editar
                }

                // Verificar si tiene permiso de escritura compartido
                var compartidos = await _documentoCompartidoRepository.ObtenerTodosAsync();
                var compartido = compartidos.FirstOrDefault(dc => 
                    dc.DocumentoId == documentoId && 
                    dc.UsuarioCompartidoId == usuarioId &&
                    dc.Permiso.ToLower() == "escritura");
                
                return compartido != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al verificar permiso de edición para documento {documentoId}");
                return false;
            }
        }
    }
}