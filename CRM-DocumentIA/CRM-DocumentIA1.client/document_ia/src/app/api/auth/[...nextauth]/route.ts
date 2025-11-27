// /pages/api/auth/[...nextauth].ts
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
  } catch {
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
        // FLUJO DE 2FA (cuando ya tienes el token del backend)
        if (credentials?.token) {
          let id = credentials.id ?? null;
          let role = credentials.role ?? null;
          let email = credentials.email ?? null;
          let name = credentials.name ?? null;

          if (!id || !role || !email || !name) {
            const payload = safeParseJwt(credentials.token);
            if (payload) {
              id = id || payload.sub || payload.nameidentifier || payload.nameid || payload.id || payload.userId || payload.user_id || null;
              email = email || payload.email || payload.upn || null;
              name = name || payload.name || null;
              role = role || payload.role || payload.roles || payload.rolNombre || payload.rol || null;
            }
          }

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
            return null;
          }

          const data = await res.json();

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
        } catch {
          return null;
        }
      },
    }),
  ],

  callbacks: {
    async signIn({ user, account }) {
      // LOGIN SOCIAL GOOGLE
      if (account?.provider === "google") {
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
            return false;
          }

          const data = await res.json();

          const usuario = data.usuario || {};
          const role =
            (usuario?.rol && (usuario.rol.nombre ?? usuario.rol)) ||
            usuario?.rolNombre ||
            usuario?.rolId ||
            data?.rol ||
            null;

          user.id = String(usuario?.id ?? user.id ?? "");
          user.name = usuario?.nombre ?? user.name;
          user.email = usuario?.email ?? user.email;
          user.role = role ?? user.role;
          user.token = data.token ?? user.token;

          return true;
        } catch {
          return false;
        }
      }

      return true;
    },

    async jwt({ token, user }) {
      if (user) {
        token.id = user.id ?? token.id;
        token.role = user.role ?? token.role;
        token.email = user.email ?? token.email;
        token.name = user.name ?? token.name;
        token.accessToken = user.token ?? token.accessToken;
      }

      return token;
    },

    async session({ session, token }) {
      session.user = {
        id: token.id,
        email: token.email,
        name: token.name,
        role: token.role,
      };

      session.accessToken = token.accessToken;

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