// types/next-auth.d.ts
import NextAuth from "next-auth";

declare module "next-auth" {
  interface Session {
    user: {
      id: string;  // Hacer id requerido
      email?: string | null;
      role?: string;
      name?: string | null;
      image?: string | null;
    };
    accessToken?: string;  // Token JWT de tu backend .NET
    expires: string;       // Asegúrate de incluir expires
  }
  
  interface User {
    id: string;           // Hacer id requerido
    email?: string | null;
    role?: string;
    name?: string | null;
    image?: string | null;
    accessToken?: string; // Debe llamarse accessToken, no token
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    id: string;           // Asegurar que id esté en el JWT
    email?: string;
    role?: string;
    name?: string;
    accessToken?: string; // Token JWT de tu backend .NET
  }
}