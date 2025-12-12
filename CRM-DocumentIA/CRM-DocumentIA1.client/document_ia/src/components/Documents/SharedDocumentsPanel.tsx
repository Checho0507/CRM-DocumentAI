/* eslint-disable @typescript-eslint/no-explicit-any */
"use client";

import { useState, useEffect } from "react";
import axios from "axios";
import { API_ROUTES, buildApiUrl } from "@/lib/apiRoutes"; // Importamos buildApiUrl
import { useSession } from "next-auth/react";
import { 
  FaUserFriends, 
  FaEnvelope, 
  FaCalendarAlt, 
  FaDownload, 
  FaEye,
  FaFilePdf,
  FaFileWord,
  FaFileExcel,
  FaShareAlt
} from "react-icons/fa";

interface SharedDocument {
  id: number;
  documentoId: number;
  nombreDocumento: string;
  tipoDocumento: string;
  tamañoDocumento: number;
  fechaSubida: string;
  compartidoPor: string;
  emailCompartidoPor: string;
  fechaCompartido: string;
  permiso: string;
  mensaje?: string;
  estadoProcesamiento: string;
  procesado: boolean;
}

interface MySharedDocument {
  id: number;
  nombreDocumento: string;
  nombreUsuarioCompartido: string;
  email: string;
  fechaCompartido: string;
  permiso: string;
  mensaje?: string;
}

export default function SharedDocumentsPanel() {
  const { data: session } = useSession();
  const [sharedDocuments, setSharedDocuments] = useState<SharedDocument[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'received' | 'shared'>('received');
  const [mySharedDocuments, setMySharedDocuments] = useState<MySharedDocument[]>([]);

  const fetchSharedDocuments = async () => {
    if (!session?.accessToken) {
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError(null);
      
      let response;
      
      if (activeTab === 'received') {
        // ✅ Usar la ruta correcta
        response = await axios.get(API_ROUTES.GET_DOCUMENTOS_CONMIGO, {
          headers: {
            Authorization: `Bearer ${session.accessToken}`,
          },
        });
        
        // Ajustar según la estructura de respuesta
        if (response.data && response.data.documentos) {
          setSharedDocuments(response.data.documentos);
        } else if (Array.isArray(response.data)) {
          setSharedDocuments(response.data);
        } else {
          console.warn("Formato de respuesta inesperado:", response.data);
          setSharedDocuments([]);
        }
      } else {
        // ✅ Usar la ruta correcta
        response = await axios.get(API_ROUTES.GET_DOCUMENTOS_MIOS, {
          headers: {
            Authorization: `Bearer ${session.accessToken}`,
          },
        });
        
        // Ajustar según la estructura de respuesta
        if (response.data && response.data.documentos) {
          setMySharedDocuments(response.data.documentos);
        } else if (Array.isArray(response.data)) {
          setMySharedDocuments(response.data);
        } else {
          console.warn("Formato de respuesta inesperado:", response.data);
          setMySharedDocuments([]);
        }
      }
      
    } catch (error: any) {
      console.error("Error al obtener documentos compartidos:", error);
      
      // Manejo mejorado de errores
      if (error.response) {
        if (error.response.status === 401) {
          setError("Tu sesión ha expirado. Por favor, vuelve a iniciar sesión.");
        } else if (error.response.status === 403) {
          setError("No tienes permisos para ver estos documentos.");
        } else if (error.response.status === 404) {
          setError("El endpoint no fue encontrado. Verifica la ruta.");
        } else if (error.response.status === 500) {
          setError("Error del servidor. Por favor, intenta más tarde.");
        } else {
          setError(`Error ${error.response.status}: ${error.response.data?.message || "Error desconocido"}`);
        }
      } else if (error.request) {
        setError("No se pudo conectar con el servidor. Verifica tu conexión.");
      } else {
        setError("Error al realizar la solicitud.");
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSharedDocuments();
  }, [session, activeTab]);

  const formatFileSize = (bytes: number): string => {
    if (!bytes) return "0 KB";
    if (bytes < 1024 * 1024) {
      return `${(bytes / 1024).toFixed(1)} KB`;
    }
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  };

  const getDocumentIcon = (fileName: string) => {
    if (!fileName) return <FaFilePdf className="text-gray-500" />;
    
    const lowerName = fileName.toLowerCase();
    if (lowerName.endsWith('.pdf')) return <FaFilePdf className="text-red-500" />;
    if (lowerName.endsWith('.docx') || lowerName.endsWith('.doc')) return <FaFileWord className="text-blue-500" />;
    if (lowerName.endsWith('.xlsx') || lowerName.endsWith('.xls')) return <FaFileExcel className="text-green-500" />;
    return <FaFilePdf className="text-gray-500" />;
  };

  const handleDownload = async (documentId: number, documentName: string) => {
    if (!session?.accessToken) {
      setError("No hay sesión activa");
      return;
    }

    try {
      // ✅ CORREGIDO: Usar buildApiUrl.downloadDocument para construir la URL
      const downloadUrl = buildApiUrl.downloadDocument(documentId);
      
      const response = await axios.get(downloadUrl, {
        headers: {
          Authorization: `Bearer ${session.accessToken}`,
        },
        responseType: 'blob',
      });

      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', documentName);
      document.body.appendChild(link);
      link.click();
      
      // Limpiar recursos
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      
      // Mensaje de éxito
      setError(null);
    } catch (error: any) {
      console.error("Error al descargar documento:", error);
      setError("Error al descargar el documento. Verifica que tengas permisos.");
    }
  };

  // Función para eliminar documento compartido (solo en la pestaña "Compartidos por mí")
  const handleRemoveShared = async (shareId: number) => {
    if (!session?.accessToken || !confirm("¿Estás seguro de que deseas dejar de compartir este documento?")) {
      return;
    }

    try {
      // ✅ Usar buildApiUrl.deleteDocumentoCompartido
      await axios.delete(buildApiUrl.deleteDocumentoCompartido(shareId), {
        headers: {
          Authorization: `Bearer ${session.accessToken}`,
        },
      });

      // Actualizar la lista de documentos compartidos por mí
      setMySharedDocuments(prev => prev.filter(doc => doc.id !== shareId));
      
      // Mostrar mensaje de éxito (podrías agregar un estado para mensajes positivos)
      console.log("Documento compartido eliminado exitosamente");
      
    } catch (error: any) {
      console.error("Error al eliminar documento compartido:", error);
      setError("Error al dejar de compartir el documento.");
    }
  };

  if (loading && sharedDocuments.length === 0 && mySharedDocuments.length === 0) {
    return (
      <div className="mt-8">
        <div className="bg-white rounded-xl shadow">
          <div className="p-6">
            <div className="animate-pulse">
              <div className="h-6 bg-gray-200 rounded w-1/4 mb-4"></div>
              <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
              <div className="h-4 bg-gray-200 rounded w-1/2"></div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="mt-8">
      <div className="bg-white rounded-xl shadow">
        <div className="p-6 border-b border-gray-200">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div className="flex items-center gap-2">
              <FaUserFriends className="text-blue-600" />
              <h3 className="text-lg font-semibold text-gray-800">
                Documentos compartidos
              </h3>
            </div>
            
            <div className="flex border border-gray-300 rounded-lg overflow-hidden">
              <button
                onClick={() => setActiveTab('received')}
                className={`px-4 py-2 text-sm font-medium ${
                  activeTab === 'received'
                    ? 'bg-blue-600 text-white'
                    : 'bg-white text-gray-700 hover:bg-gray-50'
                }`}
              >
                Recibidos
              </button>
              <button
                onClick={() => setActiveTab('shared')}
                className={`px-4 py-2 text-sm font-medium ${
                  activeTab === 'shared'
                    ? 'bg-blue-600 text-white'
                    : 'bg-white text-gray-700 hover:bg-gray-50'
                }`}
              >
                Compartidos por mí
              </button>
            </div>
          </div>
        </div>

        <div className="p-6">
          {error && (
            <div className="mb-4 p-3 bg-red-100 text-red-700 rounded-lg flex justify-between items-center">
              <span>{error}</span>
              <button
                onClick={fetchSharedDocuments}
                className="ml-2 px-3 py-1 bg-red-600 text-white text-sm rounded hover:bg-red-700"
              >
                Reintentar
              </button>
            </div>
          )}

          {activeTab === 'received' ? (
            <>
              {sharedDocuments.length === 0 ? (
                <div className="text-center py-8">
                  <div className="inline-flex items-center justify-center w-16 h-16 bg-gray-100 rounded-full mb-4">
                    <FaUserFriends className="text-2xl text-gray-400" />
                  </div>
                  <h4 className="text-lg font-medium text-gray-700 mb-2">
                    {loading ? "Cargando..." : "No tienes documentos compartidos"}
                  </h4>
                  <p className="text-gray-500">
                    Cuando alguien comparta un documento contigo, aparecerá aquí.
                  </p>
                </div>
              ) : (
                <div className="space-y-4">
                  {sharedDocuments.map((doc) => (
                    <div
                      key={doc.id}
                      className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition"
                    >
                      <div className="flex items-start gap-4">
                        <div className="text-3xl">
                          {getDocumentIcon(doc.nombreDocumento)}
                        </div>
                        
                        <div className="flex-1 min-w-0">
                          <div className="flex justify-between items-start">
                            <div>
                              <h4 className="font-semibold text-gray-800 truncate">
                                {doc.nombreDocumento || "Documento sin nombre"}
                              </h4>
                              <div className="flex flex-wrap items-center gap-3 mt-1 text-sm text-gray-600">
                                <span className="flex items-center gap-1">
                                  <FaEnvelope className="text-xs" />
                                  {doc.emailCompartidoPor || "Email no disponible"}
                                </span>
                                <span className="flex items-center gap-1">
                                  <FaCalendarAlt className="text-xs" />
                                  {doc.fechaCompartido ? new Date(doc.fechaCompartido).toLocaleDateString() : "Fecha no disponible"}
                                </span>
                                <span className={`px-2 py-0.5 rounded text-xs ${
                                  doc.permiso === 'escritura' 
                                    ? 'bg-yellow-100 text-yellow-800' 
                                    : 'bg-blue-100 text-blue-800'
                                }`}>
                                  {doc.permiso === 'escritura' ? 'Editar' : 'Solo lectura'}
                                </span>
                              </div>
                            </div>
                            
                            <div className="flex gap-2">
                              <button
                                onClick={() => handleDownload(doc.documentoId, doc.nombreDocumento || `documento-${doc.documentoId}`)}
                                className="px-3 py-1 bg-green-100 text-green-700 text-sm rounded hover:bg-green-200 flex items-center gap-1"
                                title="Descargar"
                              >
                                <FaDownload />
                              </button>
                              <button
                                className="px-3 py-1 bg-blue-100 text-blue-700 text-sm rounded hover:bg-blue-200 flex items-center gap-1"
                                title="Ver detalles"
                                onClick={() => {
                                  // Aquí podrías implementar la vista de detalles
                                  console.log("Ver detalles del documento:", doc.documentoId);
                                }}
                              >
                                <FaEye />
                              </button>
                            </div>
                          </div>
                          
                          {doc.mensaje && (
                            <div className="mt-2 text-sm text-gray-500 bg-gray-50 p-2 rounded">
                              <span className="font-medium">Mensaje: </span>
                              {doc.mensaje}
                            </div>
                          )}
                          
                          <div className="flex flex-wrap items-center gap-4 mt-2 text-xs text-gray-500">
                            <span>Tamaño: {formatFileSize(doc.tamañoDocumento)}</span>
                            <span>Subido: {doc.fechaSubida ? new Date(doc.fechaSubida).toLocaleDateString() : "Fecha no disponible"}</span>
                            <span className={`px-2 py-0.5 rounded ${
                              doc.estadoProcesamiento === 'completado' 
                                ? 'bg-green-100 text-green-800' 
                                : doc.estadoProcesamiento === 'procesando'
                                ? 'bg-yellow-100 text-yellow-800'
                                : 'bg-red-100 text-red-800'
                            }`}>
                              {doc.estadoProcesamiento || "pendiente"}
                            </span>
                          </div>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </>
          ) : (
            <>
              {mySharedDocuments.length === 0 ? (
                <div className="text-center py-8">
                  <div className="inline-flex items-center justify-center w-16 h-16 bg-gray-100 rounded-full mb-4">
                    <FaShareAlt className="text-2xl text-gray-400" />
                  </div>
                  <h4 className="text-lg font-medium text-gray-700 mb-2">
                    {loading ? "Cargando..." : "No has compartido documentos"}
                  </h4>
                  <p className="text-gray-500">
                    Cuando compartas un documento con alguien, aparecerá aquí.
                  </p>
                </div>
              ) : (
                <div className="space-y-4">
                  {mySharedDocuments.map((doc) => (
                    <div
                      key={doc.id}
                      className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition"
                    >
                      <div className="flex items-start justify-between gap-4">
                        <div className="flex items-start gap-4 flex-1">
                          <div className="text-3xl">
                            {getDocumentIcon(doc.nombreDocumento)}
                          </div>
                          
                          <div className="flex-1 min-w-0">
                            <div className="flex justify-between items-start">
                              <div>
                                <h4 className="font-semibold text-gray-800 truncate">
                                  {doc.nombreDocumento || "Documento sin nombre"}
                                </h4>
                                <div className="flex flex-wrap items-center gap-3 mt-1 text-sm text-gray-600">
                                  <span className="flex items-center gap-1">
                                    <FaEnvelope className="text-xs" />
                                    Compartido con: {doc.nombreUsuarioCompartido || "Usuario"} ({doc.email || "sin email"})
                                  </span>
                                  <span className="flex items-center gap-1">
                                    <FaCalendarAlt className="text-xs" />
                                    {doc.fechaCompartido ? new Date(doc.fechaCompartido).toLocaleDateString() : "Fecha no disponible"}
                                  </span>
                                  <span className={`px-2 py-0.5 rounded text-xs ${
                                    doc.permiso === 'escritura' 
                                      ? 'bg-yellow-100 text-yellow-800' 
                                      : 'bg-blue-100 text-blue-800'
                                  }`}>
                                    {doc.permiso === 'escritura' ? 'Editar' : 'Solo lectura'}
                                  </span>
                                </div>
                              </div>
                            </div>
                            
                            {doc.mensaje && (
                              <div className="mt-2 text-sm text-gray-500 bg-gray-50 p-2 rounded">
                                <span className="font-medium">Tu mensaje: </span>
                                {doc.mensaje}
                              </div>
                            )}
                          </div>
                        </div>
                        
                        <div className="flex gap-2">
                          <button
                            onClick={() => handleRemoveShared(doc.id)}
                            className="px-3 py-1 bg-red-100 text-red-700 text-sm rounded hover:bg-red-200"
                            title="Dejar de compartir"
                          >
                            Eliminar
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
}