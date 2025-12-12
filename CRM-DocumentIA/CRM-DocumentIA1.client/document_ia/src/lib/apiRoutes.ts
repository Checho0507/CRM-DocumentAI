// @/lib/apiRoutes.ts

const BACKEND_BASE_URL = "http://localhost:5058";

export const API_ROUTES = {
  // ============ AUTENTICACIÓN ============
  LOGIN: `${BACKEND_BASE_URL}/api/auth/login`,
  REGISTER: `${BACKEND_BASE_URL}/api/auth/register`,
  SOCIAL_LOGIN: `${BACKEND_BASE_URL}/api/auth/social-login`,
  SEND_2FA_CODE: `${BACKEND_BASE_URL}/api/auth/send-2fa-code`,
  VERIFY_2FA: `${BACKEND_BASE_URL}/api/auth/verify-2fa-code`,

  COMPARTIR_DOCUMENTO: `${BACKEND_BASE_URL}/api/DocumentoCompartido`,
  GET_DOCUMENTOS_CONMIGO: `${BACKEND_BASE_URL}/api/DocumentoCompartido/conmigo`,
  GET_DOCUMENTOS_MIOS: `${BACKEND_BASE_URL}/api/DocumentoCompartido/mios`,

  // ============ DOCUMENTOS ============
  UPLOAD_DOCUMENT: `${BACKEND_BASE_URL}/api/Documento/upload`,
  
  // Obtener documentos del usuario actual
  GET_MY_DOCUMENTS: `${BACKEND_BASE_URL}/api/Documento/mis-documentos`,
  
  // Obtener documentos de un usuario específico (para admin)
  GET_USER_DOCUMENTS: `${BACKEND_BASE_URL}/api/Documento/usuario/`, // Se debe agregar el ID del usuario al final
  
  GET_DOCUMENT_BY_ID: (documentoId: string | number) => 
    `${BACKEND_BASE_URL}/api/Documento/${documentoId}`,
  
  DELETE_DOCUMENT: (documentoId: string | number) => 
    `${BACKEND_BASE_URL}/api/Documento/${documentoId}`,
  
  GET_DOCUMENTS_BY_STATUS: (estado: string) => 
    `${BACKEND_BASE_URL}/api/Documento/estado/${estado}`,
  
  GET_DOCUMENT_PROCESSES: (documentoId: string | number) => 
    `${BACKEND_BASE_URL}/api/Documento/${documentoId}/procesos`,
  
  DOWNLOAD_DOCUMENT: (documentoId: string | number) => 
    `${BACKEND_BASE_URL}/api/Documento/${documentoId}/download`,

  // ============ USUARIO ============
  GET_PERFIL: `${BACKEND_BASE_URL}/api/Usuario/perfil`,
  
  BUSCAR_USUARIOS: `${BACKEND_BASE_URL}/api/Usuario/buscar`,

  // ============ ESTADÍSTICAS ============
  GET_USER_STATS: (usuarioId: string | number) => 
    `${BACKEND_BASE_URL}/api/Documento/stats/usuario/${usuarioId}`,
  
  UPDATE_DOCUMENT_STATUS: (documentoId: string | number) => 
    `${BACKEND_BASE_URL}/api/Documento/${documentoId}/estado`,

  // ============ CHATS ============
  ASK_CHATS: `${BACKEND_BASE_URL}/api/Chat/user`,
  HISTO_CHATS: `${BACKEND_BASE_URL}/api/Chat`,
  DELETE_CHAT: (chatId: string | number) => 
    `${BACKEND_BASE_URL}/api/Chat/${chatId}`,

  // ============ INSIGHTS ============
  INSIGHT_ASK: `${BACKEND_BASE_URL}/api/InsightsHisto/ask`,

  // ============ ANALYTICS ============
  SUMMARY_CHART: `${BACKEND_BASE_URL}/api/Analytics/summary`,
  QUERY_DAILLY: `${BACKEND_BASE_URL}/api/Analytics/consultas-por-dia`,
  DOCS_STATUS: `${BACKEND_BASE_URL}/api/Analytics/documentos-por-estado`,
  DOCS_TYPES: `${BACKEND_BASE_URL}/api/Analytics/documentos-por-tipo`,

  // ============ DOCUMENTOS COMPARTIDOS ============
  // CORREGIDO: Las rutas exactas que muestra Swagger
  DOCUMENTO_COMPARTIDO: (compartidoId: string | number) => 
    `${BACKEND_BASE_URL}/api/DocumentoCompartido/${compartidoId}`,
  VERIFICAR_PERMISO: (documentoId: string | number) => 
    `${BACKEND_BASE_URL}/api/DocumentoCompartido/verificar/${documentoId}`,
  USUARIOS_COMPARTIDOS: (documentoId: string | number) =>
    `${BACKEND_BASE_URL}/api/DocumentoCompartido/documento/${documentoId}`,

  // ============ ALTERNATIVOS (para compatibilidad) ============
  DOCUMENTOS_COMPARTIDOS_CONMIGO: `${BACKEND_BASE_URL}/api/DocumentoCompartido/conmigo`,
  DOCUMENTOS_QUE_HE_COMPARTIDO: `${BACKEND_BASE_URL}/api/DocumentoCompartido/mios`,
};

// 🔥 Helper functions
export const buildApiUrl = {
  // Documentos
  getDocumentById: (documentoId: string | number) => API_ROUTES.GET_DOCUMENT_BY_ID(documentoId),
  deleteDocument: (documentoId: string | number) => API_ROUTES.DELETE_DOCUMENT(documentoId),
  getDocumentsByStatus: (estado: string) => API_ROUTES.GET_DOCUMENTS_BY_STATUS(estado),
  getDocumentProcesses: (documentoId: string | number) => API_ROUTES.GET_DOCUMENT_PROCESSES(documentoId),
  updateDocumentStatus: (documentoId: string | number) => API_ROUTES.UPDATE_DOCUMENT_STATUS(documentoId),
  downloadDocument: (documentoId: string | number) => API_ROUTES.DOWNLOAD_DOCUMENT(documentoId),
  getUserStats: (usuarioId: string | number) => API_ROUTES.GET_USER_STATS(usuarioId),
  
  // Documentos compartidos
  deleteDocumentoCompartido: (compartidoId: string | number) => API_ROUTES.DOCUMENTO_COMPARTIDO(compartidoId),
  verificarPermiso: (documentoId: string | number) => API_ROUTES.VERIFICAR_PERMISO(documentoId),
  usuariosCompartidos: (documentoId: string | number) => API_ROUTES.USUARIOS_COMPARTIDOS(documentoId),
  
  // Chats
  deleteChat: (chatId: string | number) => API_ROUTES.DELETE_CHAT(chatId),
  
  // Usuarios
  buscarUsuarios: (query: string) => `${API_ROUTES.BUSCAR_USUARIOS}?q=${encodeURIComponent(query)}`,
};