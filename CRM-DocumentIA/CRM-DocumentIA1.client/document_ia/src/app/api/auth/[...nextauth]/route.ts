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
          console.log("🔐 Attempting credentials login:", credentials?.email);
          
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
          
          const user = await res.json();
          console.log("✅ Login successful:", user.email);
          return user;
        } catch (error) {
          console.error("🚨 Authorize error:", error);
          return null;
        }
      },
    }),
  ],

  callbacks: {
    async signIn({ user, account, profile }) {
      console.log("🔐 SignIn callback:", { 
        email: user?.email, 
        provider: account?.provider 
      });

      if (account?.provider === "google") {
        try {
          console.log("🔍 Checking Google user in database...");
          
          const res = await fetch(API_ROUTES.CHECK_USER, {
            method: "POST",
            headers: { 
              "Content-Type": "application/json",
            },
            body: JSON.stringify({ 
              email: user.email,
              name: user.name,
              image: user.image 
            }),
          });

          console.log("📊 Check user status:", res.status);

          if (!res.ok) {
            console.log("❌ User not found, redirecting to register");
            // IMPORTANTE: Retornar string para redirección
            return `/register?email=${encodeURIComponent(user.email || '')}&name=${encodeURIComponent(user.name || '')}`;
          }

          const userData = await res.json();
          console.log("✅ User found:", userData);
          
          // Asignar datos del usuario
          user.id = userData.id;
          user.role = userData.role;
          user.token = userData.token;

          return true;
        } catch (error) {
          console.error("🚨 Error in Google signIn:", error);
          return `/register?error=auth_failed&email=${encodeURIComponent(user.email || '')}`;
        }
      }

      return true;
    },

    async jwt({ token, user, account }) {
      // Primer login
      if (user) {
        token.id = String(user.id);
        token.email = user.email;
        token.role = user.role;
        token.accessToken = user.token || account?.access_token;
      }
      
      return token;
    },

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

    async redirect({ url, baseUrl }) {
      console.log("🔄 Redirect from:", url, "to base:", baseUrl);
      
      // Si es una URL de error, redirigir a login
      if (url.includes('/api/auth/error')) {
        return `${baseUrl}/login?error=auth_failed`;
      }
      
      // Si es una URL de registro, permitirla
      if (url.includes('/register')) {
        return url.startsWith('/') ? `${baseUrl}${url}` : url;
      }
      
      // Redirigir a dashboard por defecto
      if (url === `${baseUrl}/api/auth/signin` || url === `${baseUrl}/login`) {
        return `${baseUrl}/dashboard`;
      }
      
      return url.startsWith('/') ? `${baseUrl}${url}` : url;
    },
  },

  pages: {
    signIn: "/login",
    signOut: "/login",
    error: "/auth/error",
    newUser: "/register"
  },

  session: {
    strategy: "jwt",
    maxAge: 30 * 24 * 60 * 60, // 30 días
  },

  debug: process.env.NODE_ENV === "development",
  secret: process.env.NEXTAUTH_SECRET,
});

export { handler as GET, handler as POST };