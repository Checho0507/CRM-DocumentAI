// /lib/apiRoutes.ts (Crear este archivo)

const BACKEND_BASE_URL = "http://localhost:5058";

export const API_ROUTES = {
  // Rutas de Autenticación
  LOGIN: `${BACKEND_BASE_URL}/api/auth/login`,
  REGISTER: `${BACKEND_BASE_URL}/api/auth/register`,
  SOCIAL_LOGIN: `${BACKEND_BASE_URL}/api/auth/social-login`,
  // Rutas de Documentos
  UPLOAD_DOCUMENT: `${BACKEND_BASE_URL}/api/documento/upload`,
  GET_PERFIL: `${BACKEND_BASE_URL}/api/usuario/perfil`,
};