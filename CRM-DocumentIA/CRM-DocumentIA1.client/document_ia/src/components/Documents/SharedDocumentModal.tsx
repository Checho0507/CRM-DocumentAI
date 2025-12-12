"use client";

import { useState, useEffect } from "react";
import axios from "axios";
import { API_ROUTES } from "@/lib/apiRoutes";
import { useSession } from "next-auth/react";
import {
  FaTimes,
  FaCopy,
  FaEnvelope,
  FaLink,
  FaLock,
} from "react-icons/fa";

interface ShareDocumentModalProps {
  isOpen: boolean;
  onClose: () => void;
  documentId: string;
  documentName: string;
  onSuccess?: () => void;
}

interface User {
  id: number;
  nombre: string;
  email: string;
  rolNombre: string;
}

export default function ShareDocumentModal({
  isOpen,
  onClose,
  documentId,
  documentName,
  onSuccess
}: ShareDocumentModalProps) {
  const { data: session } = useSession();
  const [email, setEmail] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const [searchResults, setSearchResults] = useState<User[]>([]);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [permissions, setPermissions] = useState({
    permiso: "lectura", // lectura, escritura
    mensaje: ""
  });
  const [isLoading, setIsLoading] = useState(false);
  const [isSearching, setIsSearching] = useState(false);
  const [shareLink, setShareLink] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Buscar usuarios cuando cambia el término de búsqueda
  useEffect(() => {
    const searchUsers = async () => {
      if (searchTerm.length < 2) {
        setSearchResults([]);
        return;
      }

      setIsSearching(true);
      try {
        const response = await axios.get(
          `${API_ROUTES.BUSCAR_USUARIOS}?search=${encodeURIComponent(searchTerm)}`,
          {
            headers: {
              Authorization: `Bearer ${session?.accessToken}`,
            },
          }
        );
        
        setSearchResults(response.data);
      } catch (err) {
        console.error("Error buscando usuarios:", err);
        setSearchResults([]);
      } finally {
        setIsSearching(false);
      }
    };

    const debounceTimer = setTimeout(searchUsers, 300);
    return () => clearTimeout(debounceTimer);
  }, [searchTerm, session]);

  const handleShare = async () => {
    if (!email) {
      setError("Debes ingresar un email");
      return;
    }

    setIsLoading(true);
    setError(null);
    setSuccess(null);

    try {
      let usuarioCompartidoId: number;

      // Si ya tenemos un usuario seleccionado, usar su ID
      if (selectedUser) {
        usuarioCompartidoId = selectedUser.id;
      } else {
        // Si no, buscar el usuario por email para obtener su ID
        try {
          const buscarResponse = await axios.get(
            `http://localhost:5058/api/Usuario/buscar-por-email?email=${encodeURIComponent(email)}`,
            {
              headers: {
                Authorization: `Bearer ${session?.accessToken}`,
              },
            }
          );
          
          usuarioCompartidoId = buscarResponse.data.id;
        } catch (buscarError: any) {
          if (buscarError.response?.status === 404) {
            setError("Usuario no encontrado. Verifica el email e intenta nuevamente.");
            setIsLoading(false);
            return;
          }
          throw buscarError;
        }
      }

      // Compartir por email
      const payload = {
        documentoId: parseInt(documentId),
        usuarioCompartidoId: usuarioCompartidoId, // Ahora siempre es un número, nunca null
        permiso: permissions.permiso,
        mensaje: permissions.mensaje || `Documento "${documentName}" compartido contigo`
      };
      
      console.log("Payload final:", payload);

      await axios.post(API_ROUTES.COMPARTIR_DOCUMENTO, payload, {
        headers: {
          Authorization: `Bearer ${session?.accessToken}`,
          'Content-Type': 'application/json'
        },
      });

      setSuccess("Documento compartido exitosamente");
      setShareLink(`${window.location.origin}/documents/shared/${documentId}`);
      
      // Llamar callback de éxito
      if (onSuccess) {
        setTimeout(() => {
          onSuccess();
          resetForm();
        }, 2000);
      }
      
    } catch (err: any) {
      console.error("Error al compartir documento:", err);
      
      if (err.response) {
        console.error("Error response data:", err.response.data);
        setError(err.response.data?.mensaje || "Error al compartir el documento");
      } else {
        setError("Error al compartir el documento");
      }
    } finally {
      setIsLoading(false);
    }
  };

  // Función para seleccionar usuario de los resultados de búsqueda
  const handleSelectUser = (user: User) => {
    setSelectedUser(user);
    setEmail(user.email);
    setSearchResults([]);
    setSearchTerm("");
  };

  const resetForm = () => {
    setEmail("");
    setSearchTerm("");
    setSelectedUser(null);
    setSearchResults([]);
    setPermissions({
      permiso: "lectura",
      mensaje: ""
    });
    setShareLink(null);
    setError(null);
    setSuccess(null);
  };

  const handleCopyLink = () => {
    if (shareLink) {
      navigator.clipboard.writeText(shareLink);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-xl shadow-2xl max-w-md w-full max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex justify-between items-center p-6 border-b">
          <div>
            <h2 className="text-xl font-bold text-gray-800">Compartir documento</h2>
            <p className="text-sm text-gray-600 mt-1 truncate">{documentName}</p>
          </div>
          <button
            onClick={() => {
              resetForm();
              onClose();
            }}
            className="p-2 hover:bg-gray-100 rounded-full transition"
          >
            <FaTimes className="text-gray-500" />
          </button>
        </div>

        {/* Contenido */}
        <div className="p-6">
          {success ? (
            // Mostrar enlace compartido
            <div className="space-y-4">
              <div className="p-4 bg-green-50 border border-green-200 rounded-lg">
                <div className="flex items-center gap-2 text-green-700 mb-2">
                  <FaEnvelope />
                  <span className="font-medium">¡Compartido exitosamente!</span>
                </div>
                <p className="text-sm text-green-600">
                  {selectedUser 
                    ? `Documento compartido con ${selectedUser.nombre} (${selectedUser.email})`
                    : `Documento compartido con ${email}`
                  }
                </p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Enlace de acceso directo
                </label>
                <div className="flex gap-2">
                  <input
                    type="text"
                    readOnly
                    value={shareLink || ""}
                    className="flex-1 px-3 py-2 border border-gray-300 rounded-lg text-sm"
                  />
                  <button
                    onClick={handleCopyLink}
                    className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 flex items-center gap-2"
                  >
                    <FaCopy />
                    {copied ? "Copiado" : "Copiar"}
                  </button>
                </div>
                <p className="text-xs text-gray-500 mt-2">
                  Comparte este enlace para dar acceso directo al documento
                </p>
              </div>

              <div className="flex justify-end gap-3 pt-4">
                <button
                  onClick={() => {
                    resetForm();
                    if (onSuccess) onSuccess();
                  }}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                >
                  Listo
                </button>
              </div>
            </div>
          ) : (
            // Formulario para compartir
            <div className="space-y-6">
              {/* Búsqueda de usuarios */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Buscar usuario registrado
                </label>
                <input
                  type="text"
                  value={searchTerm}
                  onChange={(e) => {
                    setSearchTerm(e.target.value);
                    setSelectedUser(null);
                  }}
                  placeholder="Nombre o email del usuario..."
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
                
                {/* Resultados de búsqueda */}
                {isSearching && (
                  <div className="mt-2 text-sm text-gray-500">Buscando...</div>
                )}
                
                {searchResults.length > 0 && (
                  <div className="mt-2 border border-gray-200 rounded-lg max-h-40 overflow-y-auto">
                    {searchResults.map((user) => (
                      <div
                        key={user.id}
                        onClick={() => handleSelectUser(user)}
                        className="p-3 hover:bg-gray-50 cursor-pointer border-b last:border-b-0"
                      >
                        <div className="font-medium text-gray-800">{user.nombre}</div>
                        <div className="text-sm text-gray-600">{user.email}</div>
                        <div className="text-xs text-gray-500">Rol: {user.rolNombre}</div>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              {/* O ingresar email manualmente */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <FaEnvelope className="inline mr-2" />
                  O ingresar email manualmente
                </label>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => {
                    setEmail(e.target.value);
                    setSelectedUser(null);
                    setSearchTerm("");
                  }}
                  placeholder="usuario@ejemplo.com"
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>

              {/* Estado seleccionado */}
              {(selectedUser || email) && (
                <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg">
                  <p className="font-medium text-blue-800 mb-1">
                    {selectedUser ? "Usuario seleccionado:" : "Email ingresado:"}
                  </p>
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-blue-700">
                        {selectedUser 
                          ? `${selectedUser.nombre} (${selectedUser.email}) - ID: ${selectedUser.id}`
                          : email
                        }
                      </p>
                    </div>
                    <button
                      onClick={() => {
                        setSelectedUser(null);
                        setEmail("");
                        setSearchTerm("");
                      }}
                      className="text-blue-600 hover:text-blue-800"
                    >
                      <FaTimes />
                    </button>
                  </div>
                </div>
              )}

              {/* Permisos */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-3">
                  <FaLock className="inline mr-2" />
                  Permisos
                </label>
                <div className="space-y-3">
                  <label className="flex items-center gap-3 p-3 border border-gray-200 rounded-lg hover:bg-gray-50 cursor-pointer">
                    <input
                      type="radio"
                      name="permission"
                      value="lectura"
                      checked={permissions.permiso === "lectura"}
                      onChange={(e) => setPermissions({...permissions, permiso: e.target.value})}
                      className="text-blue-600"
                    />
                    <div>
                      <p className="font-medium text-gray-800">Solo lectura</p>
                      <p className="text-sm text-gray-600">
                        El usuario podrá ver y descargar el documento, pero no editarlo
                      </p>
                    </div>
                  </label>
                  
                  <label className="flex items-center gap-3 p-3 border border-gray-200 rounded-lg hover:bg-gray-50 cursor-pointer">
                    <input
                      type="radio"
                      name="permission"
                      value="escritura"
                      checked={permissions.permiso === "escritura"}
                      onChange={(e) => setPermissions({...permissions, permiso: e.target.value})}
                      className="text-blue-600"
                    />
                    <div>
                      <p className="font-medium text-gray-800">Edición</p>
                      <p className="text-sm text-gray-600">
                        El usuario podrá ver, descargar y editar el documento
                      </p>
                    </div>
                  </label>
                </div>
              </div>

              {/* Mensaje opcional */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Mensaje personalizado (opcional)
                </label>
                <textarea
                  value={permissions.mensaje}
                  onChange={(e) => setPermissions({...permissions, mensaje: e.target.value})}
                  placeholder="Agrega un mensaje personalizado para el usuario..."
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  rows={3}
                />
              </div>

              {/* Errores */}
              {error && (
                <div className="p-3 bg-red-100 text-red-700 rounded-lg">
                  {error}
                </div>
              )}

              {/* Botones */}
              <div className="flex justify-end gap-3 pt-4">
                <button
                  type="button"
                  onClick={() => {
                    resetForm();
                    onClose();
                  }}
                  className="px-4 py-2 text-gray-600 hover:text-gray-800"
                >
                  Cancelar
                </button>
                <button
                  onClick={handleShare}
                  disabled={isLoading || !email}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                >
                  {isLoading ? (
                    <>
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                      Compartiendo...
                    </>
                  ) : (
                    <>
                      <FaLink />
                      Compartir
                    </>
                  )}
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}