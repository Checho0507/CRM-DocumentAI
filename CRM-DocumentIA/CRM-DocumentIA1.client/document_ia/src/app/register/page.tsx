"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import type { Route } from "next";
import { API_ROUTES } from "@/lib/apiRoutes";
import Link from "next/link";

export default function RegisterPage() {
  const router = useRouter();
  const [formData, setFormData] = useState({
    nombre: "",
    email: "",
    password: "",
    dobleFactorActivado: false,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target;
    setFormData({
      ...formData,
      [name]: type === "checkbox" ? checked : value,
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      const res = await fetch(API_ROUTES.REGISTER, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(formData),
      });

      console.log("🔍 Response status:", res.status);
      console.log("🔍 Response ok:", res.ok);

      if (!res.ok) {
        const errorText = await res.text();
        console.error("❌ Error en registro:", errorText);
        setError("No se pudo registrar el usuario.");
        return;
      }

      // 🔥 LEER COMO JSON
      const result = await res.json();
      console.log("🔍 Registration result:", result);
      console.log("🔍 requires2FA value:", result.requires2FA);

      alert("✅ Registro exitoso");

      // 🔥 VERIFICAR EXACTAMENTE QUÉ VALOR TIENE
      const requires2FA = result.requires2FA;

      if (requires2FA) {
        // Guardar email en localStorage
        localStorage.setItem("pending2faEmail", formData.email);

        // Redirigir a la página de verificación 2FA
        router.push(
          `/auth/2FA?email=${encodeURIComponent(formData.email)}` as Route
        );
      } else {
        router.push("/login" as Route);
      }
    } catch (error) {
      console.error("❌ Fetch error:", error);
      setError("❌ Error al conectar con el servidor.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen justify-center items-center bg-gray-100">
      <form
        onSubmit={handleSubmit}
        className="bg-white p-8 rounded-xl shadow-md w-full max-w-sm space-y-4"
      >
        <h1 className="text-2xl font-bold text-center mb-4">Registro</h1>

        {error && <p className="text-red-500 text-center">{error}</p>}

        <input
          type="text"
          name="nombre"
          placeholder="Nombre"
          onChange={handleChange}
          className="w-full border px-4 py-2 rounded"
          required
        />
        <input
          type="email"
          name="email"
          placeholder="Correo"
          onChange={handleChange}
          className="w-full border px-4 py-2 rounded"
          required
        />
        <input
          type="password"
          name="password"
          placeholder="Contraseña"
          onChange={handleChange}
          className="w-full border px-4 py-2 rounded"
          required
        />

        <div className="flex items-center gap-2">
          <input
            type="checkbox"
            name="dobleFactorActivado"
            checked={formData.dobleFactorActivado}
            onChange={handleChange}
            className="h-4 w-4"
          />
          <label htmlFor="twoFactorEnabled" className="text-sm text-gray-700">
            Activar verificación en dos pasos
          </label>
        </div>

        <button
          type="submit"
          disabled={loading}
          className={`w-full py-2 text-white rounded ${
            loading ? "bg-gray-400 cursor-not-allowed" : "bg-blue-600 hover:bg-blue-700"
          }`}
        >
          {loading ? "Registrando..." : "Registrar"}
        </button>

        {/* Enlace al login */}
        <div className="text-center mt-4">
          <p className="text-gray-600 text-sm">
            ¿Ya tienes una cuenta?{" "}
            <Link
              href="/login"
              className="text-blue-600 hover:text-blue-800 font-medium"
            >
              Inicia sesión
            </Link>
          </p>
        </div>
      </form>
    </div>
  );
}