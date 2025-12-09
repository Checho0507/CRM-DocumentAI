"use client";

import type { Route } from "next";
import { useState, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { signIn, useSession } from "next-auth/react";
import { API_ROUTES } from "@/lib/apiRoutes";
import Link from "next/link";

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const router = useRouter();
  const searchParams = useSearchParams();
  const { data: session } = useSession();

  // 🔥 NUEVO: Detectar si la sesión requiere 2FA (para Google)
  useEffect(() => {
    if (session?.requires2FA) {
      console.log("🔵 Login detectó que requiere 2FA para Google");
      localStorage.setItem("pending2faEmail", session.user?.email || "");
      router.push("/auth/2FA" as Route);
    }
  }, [session, router]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError("");

    try {
      const res = await fetch(API_ROUTES.LOGIN, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
      });

      if (!res.ok) {
        setError("Credenciales incorrectas.");
        return;
      }

      const data = await res.json();
      console.log("🔐 Login response:", data);

      // 👇 Ajuste aquí — revisamos la propiedad real del backend
      if (data.dobleFactorActivado) {
        // si tiene 2FA activo, enviamos el código
        const sendCode = await fetch(API_ROUTES.SEND_2FA_CODE, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ email }),
        });

        if (!sendCode.ok) {
          setError("No se pudo enviar el código 2FA.");
          return;
        }

        localStorage.setItem("pending2faEmail", email);
        router.push("/auth/2FA" as Route);
      } else {
        // si no tiene 2FA, hacemos login con next-auth
        const signInRes = await signIn("credentials", {
          redirect: false,
          email,
          password,
        });

        if (signInRes?.error) {
          setError("Credenciales incorrectas.");
        } else {
          router.push("/dashboard");
        }
      }
    } catch (err) {
      console.error(err);
      setError("Error al iniciar sesión");
    } finally {
      setIsLoading(false);
    }
  };

  const handleGoogleSignIn = async () => {
    try {
      setIsLoading(true);
      setError("");

      // 🔥 CORREGIDO: Usamos signIn sin callbackUrl para poder detectar el 2FA
      const result = await signIn("google", {
        redirect: false,
      });

      if (result?.error) {
        setError("Error al iniciar sesión con Google");
      }
      // Si requiere 2FA, el useEffect lo detectará automáticamente

    } catch {
      setError("Error al iniciar sesión con Google");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-500 to-blue-700">
      <div className="max-w-md w-full bg-white rounded-2xl shadow-lg p-8">
        <h2 className="text-2xl font-bold text-center mb-6">Iniciar Sesión</h2>

        {error && (
          <div className="mb-4 bg-red-100 text-red-700 px-4 py-2 rounded">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <input
            type="email"
            placeholder="Correo electrónico"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="w-full border px-4 py-3 rounded-xl"
            required
          />
          <input
            type="password"
            placeholder="Contraseña"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="w-full border px-4 py-3 rounded-xl"
            required
          />
          <button
            type="submit"
            disabled={isLoading}
            className="w-full bg-blue-600 text-white py-3 rounded-xl hover:bg-blue-700 disabled:opacity-50"
          >
            {isLoading ? "Cargando..." : "Iniciar Sesión"}
          </button>
        </form>

        <div className="mt-6">
          <button
            onClick={handleGoogleSignIn}
            disabled={isLoading}
            className="w-full border py-3 rounded-xl hover:bg-gray-100 flex items-center justify-center gap-2 disabled:opacity-50"
          >
            <svg className="w-5 h-5" viewBox="0 0 24 24">
              <path
                fill="#4285F4"
                d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
              />
              <path
                fill="#34A853"
                d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
              />
              <path
                fill="#FBBC05"
                d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
              />
              <path
                fill="#EA4335"
                d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
              />
            </svg>
            {isLoading ? "Cargando..." : "Acceder con Google"}
          </button>
        </div>
        {/* Enlace al registro */}
        <div className="text-center mt-6">
          <p className="text-gray-600 text-sm">
            ¿No tienes una cuenta?{" "}
            <Link
              href="/register"
              className="text-blue-600 hover:text-blue-800 font-medium"
            >
              Regístrate
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}