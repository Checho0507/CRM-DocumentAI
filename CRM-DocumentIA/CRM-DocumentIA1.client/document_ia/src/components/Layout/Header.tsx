"use client";

import { FaSearch, FaBell } from "react-icons/fa";
import { useSession, signIn, signOut } from "next-auth/react";
import Link from "next/link";

export default function Header() {
  const { data: session } = useSession();

  // 🎯 Lógica para obtener el nombre en mayúsculas
  const userName = session?.user?.name || '';
  const displayUserName = userName.toUpperCase();

  return (
    <header className="flex justify-between items-center bg-white p-4 rounded-xl shadow mb-6">
      {/* Barra de búsqueda */}
      <div className="flex items-center bg-gray-100 rounded-full px-4 py-2 w-72">
        <FaSearch className="text-gray-500" />
        <input
          type="text"
          placeholder="Buscar..."
          className="ml-2 bg-transparent outline-none w-full"
        />
      </div>

      {/* Acciones del usuario */}
      <div className="flex items-center gap-6">
        <FaBell className="text-gray-600 text-lg cursor-pointer" />

        {session ? (
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-full bg-blue-600 flex items-center justify-center text-white font-semibold">
              {/* Inicial en mayúsculas */}
              {displayUserName.charAt(0)}
            </div>
            {/* 🎯 Mostrar Nombre Completo en Mayúsculas */}
            <span className="font-semibold text-gray-700">{displayUserName}</span>
            <button
              onClick={() => signOut({ callbackUrl: "/login" })}
              className="ml-4 px-3 py-1 text-sm bg-red-500 text-white rounded-lg hover:bg-red-600 transition duration-150"
            >
              Cerrar Sesión
            </button>
          </div>
        ) : (
          <div className="flex gap-2">
            <button
              onClick={() => signIn()}
              className="px-3 py-1 text-sm bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition duration-150"
            >
              Iniciar Sesión
            </button>
            <Link
              href="/register"
              className="px-3 py-1 text-sm bg-green-500 text-white rounded-lg hover:bg-green-600 transition duration-150"
            >
              Registrarse
            </Link>
          </div>
        )}
      </div>
    </header>
  );
}
