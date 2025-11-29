// @/lib/apiRoutes.ts

const BACKEND_BASE_URL = "http://localhost:5058";

export const API_ROUTES = {
  // Rutas de Autenticación
  LOGIN: `${BACKEND_BASE_URL}/api/auth/login`,
  REGISTER: `${BACKEND_BASE_URL}/api/auth/register`,
  SOCIAL_LOGIN: `${BACKEND_BASE_URL}/api/auth/social-login`,
  SEND_2FA_CODE: `${BACKEND_BASE_URL}/api/auth/send-2fa-code`,
  VERIFY_2FA: `${BACKEND_BASE_URL}/api/auth/verify-2fa-code`,

  // Rutas de Documentos
  UPLOAD_DOCUMENT: `${BACKEND_BASE_URL}/api/Documento/upload`,
  GET_USER_DOCUMENTS: `${BACKEND_BASE_URL}/api/Documento/usuario`, // Para: /api/Documento/usuario/{usuarioId}
  GET_DOCUMENT_BY_ID: `${BACKEND_BASE_URL}/api/Documento`, // Para: /api/Documento/{id}
  DELETE_DOCUMENT: `${BACKEND_BASE_URL}/api/Documento`, // Para: /api/Documento/{id}
  GET_DOCUMENTS_BY_STATUS: `${BACKEND_BASE_URL}/api/Documento/estado`, // Para: /api/Documento/estado/{estado}
  GET_DOCUMENT_PROCESSES: `${BACKEND_BASE_URL}/api/Documento`, // Para: /api/Documento/{id}/procesos
  
  // Rutas de Usuario
  GET_PERFIL: `${BACKEND_BASE_URL}/api/usuario/perfil`,

  // Nuevas rutas agregadas al controlador
  GET_ALL_DOCUMENTS: `${BACKEND_BASE_URL}/api/Documento`,
  GET_DOCUMENT_STATUSES: `${BACKEND_BASE_URL}/api/Documento/estado`,
  UPDATE_DOCUMENT_STATUS: `${BACKEND_BASE_URL}/api/Documento`, // Para: /api/Documento/{id}/estado
  DOWNLOAD_DOCUMENT: `${BACKEND_BASE_URL}/api/Documento`, // Para: /api/Documento/{id}/download
  GET_USER_STATS: `${BACKEND_BASE_URL}/api/Documento/stats/usuario`, // Para: /api/Documento/stats/usuario/{usuarioId}
};