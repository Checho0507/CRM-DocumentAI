// src/lib/apiRoutes.ts
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL;

export const API_ROUTES = {
  LOGIN: `${API_BASE_URL}/api/users/login`,
  REGISTER: `${API_BASE_URL}/api/register`,
  CHECK_USER: `${API_BASE_URL}/api/check-user`,
  USERS: `${API_BASE_URL}/users`,
  // agrega los que necesites
};
