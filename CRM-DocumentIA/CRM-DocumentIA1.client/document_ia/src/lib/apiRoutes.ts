// /lib/apiRoutes.ts (Crear este archivo)

const BACKEND_BASE_URL = "http://localhost:5058";

export const API_ROUTES = {
  // Rutas de Autenticación
  LOGIN: `${BACKEND_BASE_URL}/api/auth/login`,
  REGISTER: `${BACKEND_BASE_URL}/api/auth/register`,
  SOCIAL_LOGIN: `${BACKEND_BASE_URL}/api/auth/social-login`,
  SEND_2FA_CODE: `${BACKEND_BASE_URL}/api/auth/send-2fa-code`,
  VERIFY_2FA: `${BACKEND_BASE_URL}/api/auth/verify-2fa-code`,

  // Rutas de Documentos
  UP_DOCUMENT: `${BACKEND_BASE_URL}/api/Documento`,
  UPLOAD_DOCUMENT: `${BACKEND_BASE_URL}/api/documento/upload`,
  GET_PERFIL: `${BACKEND_BASE_URL}/api/usuario/perfil`,
};