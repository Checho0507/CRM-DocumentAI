'use client';

import { useSession, signOut } from 'next-auth/react';

export default function Dashboard() {
  const { data: session, status } = useSession();

  if (status === 'loading') {
    return <p className="p-6">Cargando sesión...</p>;
  }

  if (!session) {
    return <p className="p-6">No estás autenticado.</p>;
  }

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold">Dashboard Principal</h1>
      <p className="mt-2">Bienvenido, {session.user?.name}</p>
      <p className="text-gray-600">{session.user?.email}</p>

      <button
        onClick={() => signOut()}
        className="mt-4 px-4 py-2 bg-red-500 text-white rounded"
      >
        Cerrar Sesión
      </button>
    </div>
  );
}
