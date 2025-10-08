import NextAuth from "next-auth";
import GoogleProvider from "next-auth/providers/google";
import CredentialsProvider from "next-auth/providers/credentials";
import { API_ROUTES } from "@/lib/apiRoutes";

const handler = NextAuth({
  providers: [
    // 🔹 Proveedor Google
    GoogleProvider({
      clientId: process.env.GOOGLE_CLIENT_ID!,
      clientSecret: process.env.GOOGLE_CLIENT_SECRET!,
    }),

    // 🔹 Proveedor de credenciales personalizadas
    CredentialsProvider({
      name: "Credentials",
      credentials: {
        email: { label: "Email", type: "text" },
        password: { label: "Password", type: "password" },
      },
      async authorize(credentials) {
        const BYPASS_AUTH = process.env.NODE_ENV === "development";

        // 🧪 MODO SIN BACKEND — Usuario Demo
        if (BYPASS_AUTH) {
          console.log("⚡ [DEV] Modo sin backend activo — Usuario Demo autorizado");
          return {
            id: "1",
            name: "Usuario Demo",
            email: credentials?.email || "demo@empresa.com",
            role: "admin",
            token: "fake-jwt-token",
          };
        }

        // 🔸 Flujo real de login (cuando ya tengas backend)
        try {
          console.log("🔐 Intentando login real:", credentials?.email);

          const res = await fetch(API_ROUTES.LOGIN, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              email: credentials?.email,
              password: credentials?.password,
            }),
          });

          if (!res.ok) {
            console.error("❌ Error en login:", res.status, await res.text());
            return null;
          }

          const user = await res.json();
          console.log("✅ Login exitoso:", user.email);
          return user;
        } catch (error) {
          console.error("🚨 Error en authorize:", error);
          return null;
        }
      },
    }),
  ],

  // 🔹 Callbacks
  callbacks: {
    async signIn({ user, account, profile }) {
      console.log("🔐 SignIn callback:", {
        email: user?.email,
        provider: account?.provider,
      });

      if (account?.provider === "google") {
        try {
          const res = await fetch(API_ROUTES.CHECK_USER, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              email: user.email,
              name: user.name,
              image: user.image,
            }),
          });

          if (!res.ok) {
            console.log("❌ Usuario no encontrado — Redirigiendo a registro");
            return `/register?email=${encodeURIComponent(user.email || "")}&name=${encodeURIComponent(user.name || "")}`;
          }

          const userData = await res.json();
          user.id = userData.id;
          user.role = userData.role;
          user.token = userData.token;
          return true;
        } catch (error) {
          console.error("🚨 Error en Google signIn:", error);
          return `/register?error=auth_failed&email=${encodeURIComponent(user.email || "")}`;
        }
      }

      return true;
    },

    async jwt({ token, user, account }) {
      // ⚡ BYPASS (modo sin backend)
      if (process.env.NODE_ENV === "development" && !user && !token.email) {
        console.log("⚡ [DEV] Inyectando token de usuario demo");
        token.id = "1";
        token.email = "demo@empresa.com";
        token.name = "Usuario Demo";
        token.role = "admin";
        token.accessToken = "fake-jwt-token";
      }

      // 🔸 Si hay usuario (primer login)
      if (user) {
        token.id = String(user.id);
        token.email = user.email;
        token.role = user.role || "admin";
        token.name = user.name;
        token.accessToken = user.token || account?.access_token || "fake-jwt-token";
      }

      return token;
    },

    async session({ session, token }) {
      // ⚡ BYPASS (modo sin backend)
      if (process.env.NODE_ENV === "development" && !token.email) {
        console.log("⚡ [DEV] Sesión demo activa");
        session.user = {
          id: "1",
          email: "demo@empresa.com",
          name: "Usuario Demo",
          role: "admin",
          image: "",
        };
        session.accessToken = "fake-jwt-token";
        return session;
      }

      // 🔸 Flujo normal
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

    async redirect({ url, baseUrl }) {
      if (url.includes("/api/auth/error")) return `${baseUrl}/login?error=auth_failed`;
      if (url.includes("/register")) return url.startsWith("/") ? `${baseUrl}${url}` : url;
      if (url === `${baseUrl}/api/auth/signin` || url === `${baseUrl}/login`) return `${baseUrl}/dashboard`;
      return url.startsWith("/") ? `${baseUrl}${url}` : url;
    },
  },

  // 🔹 Páginas personalizadas
  pages: {
    signIn: "/login",
    signOut: "/login",
    error: "/auth/error",
    newUser: "/register",
  },

  // 🔹 Configuración de sesión
  session: {
    strategy: "jwt",
    maxAge: 30 * 24 * 60 * 60, // 30 días
  },

  // 🔹 Otros
  debug: process.env.NODE_ENV === "development",
  secret: process.env.NEXTAUTH_SECRET,
});

export { handler as GET, handler as POST };
