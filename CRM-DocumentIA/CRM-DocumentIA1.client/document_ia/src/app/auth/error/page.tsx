'use client';

import { useSearchParams } from 'next/navigation';
import Link from 'next/link';

export default function AuthErrorPage() {
    const searchParams = useSearchParams();
    const error = searchParams.get('error');

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-50">
            <div className="max-w-md w-full bg-white rounded-lg shadow-md p-6">
                <div className="text-center">
                    <h1 className="text-2xl font-bold text-red-600 mb-4">
                        Error de Autenticación
                    </h1>
                    <p className="text-gray-600 mb-4">
                        {error === 'UserNotFound'
                            ? 'No encontramos una cuenta con este email. Por favor regístrate primero.'
                            : 'Ocurrió un error durante la autenticación. Por favor intenta de nuevo.'
                        }
                    </p>
                    <div className="space-y-3">
                        <Link
                            href="/register"
                            className="block w-full bg-blue-500 text-white py-2 px-4 rounded hover:bg-blue-600 transition"
                        >
                            Ir al Registro
                        </Link>
                        <Link
                            href="/login"
                            className="block w-full bg-gray-500 text-white py-2 px-4 rounded hover:bg-gray-600 transition"
                        >
                            Volver al Login
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
}