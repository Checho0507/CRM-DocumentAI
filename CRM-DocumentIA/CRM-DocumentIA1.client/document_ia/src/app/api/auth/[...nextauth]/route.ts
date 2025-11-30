// /pages/api/auth/[...nextauth].ts (CORREGIDO: extracción de id/role desde token 2FA)
import NextAuth from "next-auth";
import GoogleProvider from "next-auth/providers/google";
import CredentialsProvider from "next-auth/providers/credentials";
import { API_ROUTES } from "@/lib/apiRoutes";

function safeParseJwt(token?: string) {
  if (!token) return null;
  try {
    const parts = token.split(".");
    if (parts.length !== 3) return null;
    const payload = Buffer.from(parts[1], "base64").toString("utf8");
    return JSON.parse(payload);
  } catch (e) {
    console.error("safeParseJwt error:", e);
    return null;
  }
}

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
        token: { label: "Token", type: "text" },
        role: { label: "Role", type: "text" },
      },

      async authorize(credentials) {
        console.log("🟡 Authorize() -> credentials recibidas:", credentials);

        // FLUJO DE 2FA (cuando ya tienes el token del backend)
        if (credentials?.token) {
          console.log("🟢 Authorize() -> Login 2FA detectado");

          // Primero intenta tomar id/role desde credentials (si el frontend lo envió)
          let id = credentials.id ?? null;
          let role = credentials.role ?? null;
          let email = credentials.email ?? null;
          let name = credentials.name ?? null;

          // Si no vienen, intentamos decodificar el JWT para extraer claims
          if (!id || !role || !email || !name) {
            const payload = safeParseJwt(credentials.token);
            console.log("🟢 Authorize() -> Payload decodificado:", payload);

            if (payload) {
              // CANDIDATOS PARA ID
              id =
                id ||
                payload.sub ||
                payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ||
                payload["nameidentifier"] ||
                payload.nameid ||
                payload["id"] ||
                payload.userId ||
                payload.user_id ||
                null;

              // CANDIDATOS PARA EMAIL
              email =
                email ||
                payload.email ||
                payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] ||
                payload.upn ||
                null;

              // CANDIDATOS PARA NAME
              name =
                name ||
                payload.name ||
                payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
                null;

              // CANDIDATOS PARA ROLE
              role =
                role ||
                payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
                payload.role ||
                payload.roles ||
                payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role"] ||
                payload.rolNombre ||
                payload.rol ||
                null;
            }
          }

          console.log("🟢 Authorize() -> 2FA extraccion final:", { id, email, name, role });

          return {
            id: id ? String(id) : undefined,
            email: email ?? undefined,
            name: name ?? undefined,
            token: credentials.token,
            role: role ?? undefined,
          };
        }

        // LOGIN NORMAL (email + password)
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
            console.error("❌ Login failed:", res.status, await res.text());
            return null;
          }

          const data = await res.json();
          console.log("🟢 Login backend OK -> data:", data);

          // Normalizamos role con varios fallbacks por si tu backend devuelve distintas formas
          const usuario = data.usuario || {};
          const role =
            (usuario?.rol && (usuario.rol.nombre ?? usuario.rol)) ||
            usuario?.rolNombre ||
            usuario?.rolId ||
            data?.rol ||
            null;

          return {
            id: String(usuario.id),
            name: usuario.nombre ?? usuario.name ?? undefined,
            email: usuario.email ?? undefined,
            role: role ?? undefined,
            token: data.token,
          };
        } catch (error) {
          console.error("🚨 Authorize() error:", error);
          return null;
        }
      },
    }),
  ],

  callbacks: {
    async signIn({ user, account }) {
      console.log("🟡 signIn() -> user:", user, "| account:", account);

      // LOGIN SOCIAL GOOGLE
      if (account?.provider === "google") {
        console.log("🔵 SignIn Google detectado");
        try {
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
          console.log("🟢 Social login data:", data);

          const usuario = data.usuario || {};
          // Normalize role
          const role =
            (usuario?.rol && (usuario.rol.nombre ?? usuario.rol)) ||
            usuario?.rolNombre ||
            usuario?.rolId ||
            data?.rol ||
            null;

          // Update user with backend canonical info — NextAuth will pass this `user` into jwt()
          user.id = String(usuario?.id ?? user.id ?? "");
          user.name = usuario?.nombre ?? user.name;
          user.email = usuario?.email ?? user.email;
          user.role = role ?? user.role;
          user.token = data.token ?? user.token;

          console.log("🟢 signIn() -> user actualizado:", user);
          return true;
        } catch (error) {
          console.error("🚨 Error Google signIn():", error);
          return false;
        }
      }

      return true;
    },

    async jwt({ token, user }) {
      console.log("🟡 jwt() -> TOKEN ANTES:", token, "| user:", user);

      if (user) {
        token.id = user.id ?? token.id;
        token.role = user.role ?? token.role;
        token.email = user.email ?? token.email;
        token.name = user.name ?? token.name;
        token.accessToken = user.token ?? token.accessToken;
      }

      console.log("🟢 jwt() -> TOKEN DESPUÉS:", token);
      return token;
    },

    async session({ session, token }) {
      console.log("🟡 session() -> token recibido:", token);

      session.user = {
        id: token.id,
        email: token.email,
        name: token.name,
        role: token.role,
      };

      session.accessToken = token.accessToken;

      console.log("🟢 session() -> sesión final:", session);
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
    maxAge: 30 * 24 * 60 * 60,
  },

  debug: process.env.NODE_ENV === "development",
  secret: process.env.NEXTAUTH_SECRET,
});

export { handler as GET, handler as POST };