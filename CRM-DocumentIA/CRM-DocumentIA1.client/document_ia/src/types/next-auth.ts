// types/next-auth.d.ts
import NextAuth from "next-auth";

declare module "next-auth" {
  interface Session {
    user: {
      id?: string;
      email?: string | null;
      role?: string;
      name?: string | null;
      image?: string | null;
    };
    accessToken?: string;
  }
  interface User {
    id?: string;
    email?: string | null;
    role?: string;
    name?: string | null;
    image?: string | null;
    token?: string;
  }
}
declare module "next-auth/jwt" {
  interface JWT {
    accessToken?: string;
  }
}