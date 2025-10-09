// /pages/api/auth/[...nextauth].ts (FINAL Y CORREGIDO)

import NextAuth from "next-auth";
import GoogleProvider from "next-auth/providers/google";
import CredentialsProvider from "next-auth/providers/credentials";
import { API_ROUTES } from "@/lib/apiRoutes";

const handler = NextAuth({
  providers: [
    GoogleProvider({
      clientId: process.env.GOOGLE_CLIENT_ID!,
      clientSecret: process.env.GOOGLE_CLIENT_SECRET!,
    }),
    CredentialsProvider({
      name: "Credentials",
      credentials: {
        email: { label: "Email", type: "text" },
        password: { label: "Password", type: "password" },
      },
      async authorize(credentials) {
        try {
          // 1. Lógica de Credenciales: Perfecta, ya mapea el JWT al objeto 'user'
          const res = await fetch(API_ROUTES.LOGIN, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              email: credentials?.email,
              password: credentials?.password,
            }),
          });

          if (!res.ok) {
            console.error("❌ Login failed:", res.status, await res.text());
            return null;
          }
          
          const data = await res.json(); 
          console.log("✅ Login successful:", data.usuario.email);

          // 2. Retorna el objeto 'user' simple con el JWT adjunto
          const user = {
            id: String(data.usuario.id),
            email: data.usuario.email,
            name: data.usuario.nombre,
            role: data.usuario.rol,
            token: data.token, // 🎯 PROPIEDAD CLAVE
          };

          return user;
        } catch (error) {
          console.error("🚨 Authorize error:", error);
          return null;
        }
      },
    }),
  ],

  callbacks: {
    async signIn({ user, account }) {
      console.log("🔐 SignIn callback:", {
        email: user?.email,
        provider: account?.provider,
      });

      // 🎯 NUEVO FLUJO DE LOGIN SOCIAL (GOOGLE)
      if (account?.provider === "google") {
        try {
          console.log("🔍 Calling Social Login endpoint...");

          // 1. Llama al endpoint único que gestiona Login O Registro en el backend
          const res = await fetch(API_ROUTES.SOCIAL_LOGIN, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              email: user.email,
              name: user.name, // Solo se necesita email y name
            }),
          });

          // Si el backend no devuelve 200 OK, es un error fatal (ej. DB caída)
          if (!res.ok) {
            console.error("❌ Social Login failed:", res.status, await res.text());
            return false; 
          }

          // 2. Respuesta: { token, usuario: { id, email, ... } }
          const data = await res.json();
          console.log("✅ Social Login/Register successful:", data.usuario.email);

          // 3. Modificamos el objeto 'user' de NextAuth para adjuntar la info y el JWT
          // (Es necesario usar 'as any' porque 'user' viene tipado solo con las propiedades básicas)
          user.id = String(data.usuario.id);
          user.role = data.usuario.rol;
          user.name = data.usuario.nombre;
          user.token = data.token; // 🎯 Adjuntamos el JWT

          return true; // Permite el inicio de sesión

        } catch (error) {
          console.error("🚨 Error in Google signIn:", error);
          return false;
        }
      }

      // Para Credenciales, el 'authorize' ya devolvió el usuario con el token, así que:
      return true;
    },

    // 🎯 Callback JWT: Almacena el token del backend en el token de la sesión
    async jwt({ token, user, account }) {
      if (user) {
        token.id = String(user.id);
        token.email = user.email;
        token.role = user.role;
        // La propiedad 'token' viene de Credentials o fue adjuntada en signIn (Google)
        token.accessToken = user.token || account?.access_token;
      }
      return token;
    },

    // 🎯 Callback Session: Expone el token para que la app lo use en headers
    async session({ session, token }) {
      if (token) {
        session.user = {
          id: token.id as string,
          email: token.email as string,
          role: token.role as string,
          name: token.name as string,
          image: token.picture as string,
        };
        session.accessToken = token.accessToken as string;
      }
      return session;
    },

    // ... (El callback redirect y el resto de la configuración se mantienen) ...
    async redirect({ url, baseUrl }) {
        if (url.includes("/api/auth/error")) {
            return `${baseUrl}/login?error=auth_failed`;
        }
        if (url.includes("/register")) {
            return url.startsWith("/") ? `${baseUrl}${url}` : url;
        }
        if (url === `${baseUrl}/api/auth/signin` || url === `${baseUrl}/login`) {
            return `${baseUrl}/dashboard`;
        }
        return url.startsWith("/") ? `${baseUrl}${url}` : url;
    },
  },

  pages: {
    signIn: "/login",
    signOut: "/login",
    error: "/auth/error",
    newUser: "/register",
  },

  session: {
    strategy: "jwt",
    maxAge: 30 * 24 * 60 * 60, // 30 días
  },

  debug: process.env.NODE_ENV === "development",
  secret: process.env.NEXTAUTH_SECRET,
});

export { handler as GET, handler as POST };