// app/layout.tsx - SIN "use client" (Server Component)
import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import { getServerSession } from "next-auth";
import SessionProvider from "../components/SessionProvider";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "CRM Inteligente",
  description: "Sistema de gestión documental con IA",
};

export default async function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  // Obtiene la sesión en el servidor
  const session = await getServerSession();

  return (
    <html lang="es">
      <body className={inter.className}>
        {/* Pasa la sesión al SessionProvider */}
        <SessionProvider session={session}>
          {children}
        </SessionProvider>
      </body>
    </html>
  );
}