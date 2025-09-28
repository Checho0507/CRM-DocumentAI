"use client";
import { signIn, signOut, useSession } from "next-auth/react";

export default function LoginButtons() {
    const { data: session } = useSession();

    if (session) {
        return (
            <div className="flex items-center gap-2">
                <span>Hola {session.user?.name}</span>
                <button
                    onClick={() => signOut()}
                    className="bg-red-500 text-white px-3 py-1 rounded"
                >
                    Logout
                </button>
            </div>
        );
    }

    return (
        <div className="flex flex-col gap-2">
            <button
                onClick={() => signIn("google")}
                className="bg-blue-500 text-white px-3 py-1 rounded"
            >
                Acceder con Google
            </button>
            <button
                onClick={() => signIn("credentials", { email: "demo@test.com", password: "123456" })}
                className="bg-gray-700 text-white px-3 py-1 rounded"
            >
                Login con usuario
            </button>
        </div>
    );
}
