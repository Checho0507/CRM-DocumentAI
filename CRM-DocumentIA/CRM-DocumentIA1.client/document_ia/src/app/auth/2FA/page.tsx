"use client";


import { signIn } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { API_ROUTES } from "@/lib/apiRoutes";

export default function TwoFAPage() {
    const router = useRouter();
    const [code, setCode] = useState("");
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    const email = typeof window !== "undefined" ? localStorage.getItem("pending2faEmail") : null;

    const handleVerify = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!email) return setError("No se encontró email para verificar.");

        setLoading(true);
        setError("");

        try {
            const res = await fetch(API_ROUTES.VERIFY_2FA, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email, codigo: code }),
            });

            if (!res.ok) {
                const err = await res.json();
                setError(err.message || "Código inválido o expirado.");
                return;
            }

            const data = await res.json();
            const token = data.token;

            // 🚀 Iniciar sesión en NextAuth con el token
            const signInRes = await signIn("credentials", {
                redirect: false,
                name: data.usuario.nombre,
                email: data.usuario.email,
                token, // este token se propagará al callback JWT de NextAuth
            });

            if (signInRes?.error) {
                setError("Error al iniciar sesión después del 2FA.");
                return;
            }

            localStorage.removeItem("pending2faEmail");
            router.push("/dashboard");
        } catch (err) {
            console.error(err);
            setError("Error al verificar el código.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="flex min-h-screen items-center justify-center bg-gray-100">
            <form
                onSubmit={handleVerify}
                className="bg-white p-8 rounded-xl shadow-md w-full max-w-sm space-y-4"
            >
                <h1 className="text-2xl font-bold text-center mb-4">Verificación 2FA</h1>
                <p className="text-sm text-gray-600 text-center">
                    Se ha enviado un código a <strong>{email}</strong>
                </p>

                {error && <div className="text-red-600 text-center">{error}</div>}

                <input
                    type="text"
                    placeholder="Código de verificación"
                    value={code}
                    onChange={(e) => setCode(e.target.value)}
                    className="w-full border px-4 py-2 rounded"
                    required
                />

                <button
                    type="submit"
                    disabled={loading}
                    className={`w-full py-2 text-white rounded ${loading ? "bg-gray-400" : "bg-blue-600 hover:bg-blue-700"
                        }`}
                >
                    {loading ? "Verificando..." : "Verificar"}
                </button>
            </form>
        </div>
    );
}
