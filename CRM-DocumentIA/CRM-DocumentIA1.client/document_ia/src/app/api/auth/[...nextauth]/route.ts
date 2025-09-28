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
          const res = await fetch(API_ROUTES.LOGIN, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              email: credentials?.email,
              password: credentials?.password,
            }),
          });

          if (!res.ok) {
            console.error("Login failed:", res.status, res.statusText);
            return null;
          }
          
          const user = await res.json();
          return user;
        } catch (error) {
          console.error("Authorize error:", error);
          return null;
        }
      },
    }),
  ],

  callbacks: {
    async signIn({ user, account }) {
      console.log("🔐 SignIn callback triggered:", { 
        email: user?.email, 
        provider: account?.provider 
      });

      // Si es Google OAuth
      if (account?.provider === "google") {
        try {
          console.log("🔍 Checking user in database...");
          const res = await fetch(API_ROUTES.CHECK_USER, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              email: user.email,
              name: user.name,
            }),
          });

          console.log("📊 Check user response status:", res.status);

          if (!res.ok) {
            // Si el usuario no existe, redirigir a registro
            console.log("❌ User not found, redirecting to register");
            return "/register?error=UserNotFound";
          }

          const userData = await res.json();
          console.log("✅ User found:", userData);
          // Asignar los datos del usuario para usar en JWT
          user.id = userData.id;
          user.role = userData.role;
          user.token = userData.token;

          return true;
        } catch (error) {
          console.error("❌ Error in signIn callback:", error);
          // En caso de error, redirigir a registro
          return "/register?error=ConnectionError";
        }
      }

      return true;
    },

    async jwt({ token, user, account }) {
      console.log("🔑 JWT callback:", {
        userEmail: user?.email,
        tokenEmail: token.email
      });

      // Primer login - user está disponible
      if (user) {
        token.id = String(user.id);
        token.email = user.email;
        token.role = user.role;
        token.accessToken = user.token || account?.access_token;
      }
      
      return token;
    },

    async session({ session, token }) {
      console.log("💼 Session callback:", { 
        sessionEmail: session.user.email,
        tokenEmail: token.email 
      });

      session.user = {
        id: token.id as string,
        email: token.email as string,
        role: token.role as string,
        name: token.name as string,
        image: token.picture as string,
      };
      session.accessToken = token.accessToken as string;
      
      return session;
    },

    async redirect({ url, baseUrl }) {
      console.log("🔄 Redirect callback:", { url, baseUrl });
      
      // Si hay un error, redirigir a la página de error
      if (url.includes('/api/auth/error')) {
        return `${baseUrl}/login?error=AuthenticationFailed`;
      }
      
      // Si es una URL relativa, usar baseUrl
      if (url.startsWith('/')) return `${baseUrl}${url}`;
      // Si es una URL del mismo origen, permitirla
      else if (new URL(url).origin === baseUrl) return url;
      
      return baseUrl;
    },
  },

  pages: {
    signIn: "/login",
    error: "/auth/error", // Asegúrate de tener esta página
  },

  debug: process.env.NODE_ENV === "development",
  secret: process.env.NEXTAUTH_SECRET,
});

export { handler as GET, handler as POST };