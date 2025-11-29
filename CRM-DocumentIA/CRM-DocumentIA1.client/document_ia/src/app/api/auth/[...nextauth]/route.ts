// /pages/api/auth/[...nextauth].ts (CORREGIDO)

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
        id: { label: "Id", type: "text" },
        name: { label: "Name", type: "text" },
        email: { label: "Email", type: "text" },
        password: { label: "Password", type: "password" },
        token: { label: "Token", type: "text" }, // para 2FA
      },
      async authorize(credentials) {
        try {
          if (credentials?.token) {
            // 🔹 CORRECCIÓN: Para 2FA, también necesitamos hacer la verificación con el backend
            console.log("🔐 Verificando 2FA para:", credentials.email);
            
            const res = await fetch(API_ROUTES.VERIFY_2FA, {
              method: "POST",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify({
                email: credentials.email,
                Codigo: credentials.token,
              }),
            });

            if (!res.ok) {
              console.error("❌ 2FA verification failed:", await res.text());
              return null;
            }

            const data = await res.json();
            console.log("✅ 2FA verification successful:", data.usuario.email);

            return {
              id: String(data.usuario.id),
              email: data.usuario.email,
              name: data.usuario.nombre,
              role: data.usuario.rol,
              token: data.token,
            };
          }

          // 1. Lógica de Credenciales
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
          console.log("✅ Login successful:", data.usuario.email, data.usuario.id);

          // 2. Retorna el objeto 'user' con todas las propiedades necesarias
          const user = {
            id: String(data.usuario.id),
            email: data.usuario.email,
            name: data.usuario.nombre,
            role: data.usuario.rol,
            token: data.token,
          };
          console.log("Marlon usuarioSession authorize:", user);
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
      console.log("🔐 SignIn callback - user completo:", user);
      console.log("🔐 SignIn callback - propiedades:", {
        id: user?.id,
        name: user?.name,
        email: user?.email,
        token: user?.token,
        role: user?.role,
        provider: account?.provider,
      });

      // 🎯 FLUJO DE LOGIN SOCIAL (GOOGLE)
      if (account?.provider === "google") {
        try {
          console.log("🔍 Calling Social Login endpoint...");

          const res = await fetch(API_ROUTES.SOCIAL_LOGIN, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              email: user.email,
              name: user.name,
            }),
          });

          if (!res.ok) {
            console.error("❌ Social Login failed:", res.status, await res.text());
            return false; 
          }

          const data = await res.json();
          console.log("✅ Social Login/Register successful:", data.usuario.email);

          // 🔹 CORRECCIÓN: Actualizar todas las propiedades del usuario
          user.id = String(data.usuario.id);
          user.role = data.usuario.rol;
          user.name = data.usuario.nombre;
          user.token = data.token;
          console.log("Marlon UserSession Google actualizado:", user);
          
          return true;

        } catch (error) {
          console.error("🚨 Error in Google signIn:", error);
          return false;
        }
      }

      // Para Credenciales, el 'authorize' ya devolvió el usuario con el token
      console.log("✅ Login con credenciales exitoso, user:", user);
      return true;
    },

    // 🎯 Callback JWT: Almacena el token del backend en el token de la sesión
    async jwt({ token, user, account }) {
      console.log("🔄 JWT callback - user:", user);
      console.log("🔄 JWT callback - token actual:", token);
      
      if (user) {
        // 🔹 CORRECCIÓN: Asegurar que todas las propiedades se copien al token
        token.id = String(user.id);
        token.email = user.email;
        token.role = user.role;
        token.name = user.name;
        token.accessToken = user.token || account?.access_token;
        
        console.log("✅ JWT actualizado con user data:", {
          id: token.id,
          email: token.email,
          role: token.role,
          name: token.name,
          hasAccessToken: !!token.accessToken
        });
      }
      return token;
    },

    // 🎯 Callback Session: Expone el token para que la app lo use en headers
    async session({ session, token }) {
      console.log("📋 Session callback - token:", token);
      
      if (token) {
        session.user = {
          id: token.id as string,
          email: token.email as string,
          role: token.role as string,
          name: token.name as string,
          image: token.picture as string,
        };
        session.accessToken = token.accessToken as string;
        
        console.log("✅ Session creada:", {
          user: session.user,
          hasAccessToken: !!session.accessToken
        });
      }
      return session;
    },

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
        if (url === `${baseUrl}/api/auth/signout`) {
            return `${baseUrl}/login`;
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