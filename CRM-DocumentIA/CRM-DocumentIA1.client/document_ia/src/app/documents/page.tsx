"use client";

import { useState, useEffect } from "react";
import Layout from "@/components/Layout/Layout";
import { FaUpload } from "react-icons/fa";
import DocumentCard from "@/components/Common/DocumentCard";
import axios from "axios";
import { API_ROUTES } from "@/lib/apiRoutes";
import { useSession } from "next-auth/react";

// Interfaces TypeScript
interface BackendDocument {
  id: number;
  usuarioId: number;
  nombreArchivo: string;
  tipoDocumento: string;
  rutaArchivo?: string;
  tamañoArchivo: number;
  fechaSubida: string;
  estadoProcesamiento: string;
  procesado: boolean;
  numeroImagenes?: number;
  resumenDocumento?: string;
  archivoMetadataJson?: string;
  errorProcesamiento?: string;
  fechaProcesamiento?: string;
}

interface FrontendDocument {
  id: string;
  name: string;
  type: "PDF" | "Word" | "Excel";
  size: string;
  date: string;
}

export default function DocumentsPage() {
  const { data: session, status } = useSession();
  const [documents, setDocuments] = useState<FrontendDocument[]>([]);
  const [dragActive, setDragActive] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [loading, setLoading] = useState(true);

  // 🔹 Determinar el tipo de documento basado en el nombre del archivo
  const determineDocumentType = (fileName: string): "PDF" | "Word" | "Excel" => {
    const lowerFileName = fileName.toLowerCase();
    
    if (lowerFileName.endsWith(".pdf")) {
      return "PDF";
    } else if (lowerFileName.endsWith(".docx") || lowerFileName.endsWith(".doc")) {
      return "Word";
    } else if (lowerFileName.endsWith(".xlsx") || lowerFileName.endsWith(".xls")) {
      return "Excel";
    }
    return "PDF"; // Por defecto
  };

  // 🔹 Formatear el tamaño del archivo
  const formatFileSize = (doc: BackendDocument): string => {
    if (doc.tamañoArchivo) {
      return `${(doc.tamañoArchivo / (1024 * 1024)).toFixed(1)} MB`;
    }
    return "0 MB";
  };

  // 🔹 Formatear la fecha
  const formatDate = (dateString: string): string => {
    return new Date(dateString).toISOString().split('T')[0];
  };

  // 🔹 Obtener documentos del usuario desde la BD
  const fetchUserDocuments = async () => {
    if (!session?.user?.id) {
      return;
    }

    try {
      setLoading(true);
      const response = await axios.get<BackendDocument[]>(
        `${API_ROUTES.GET_USER_DOCUMENTS}/${session.user.id}`,
        {
          headers: {
            Authorization: `Bearer ${session.accessToken}`,
          },
        }
      );

      console.log("📄 Documentos cargados desde BD:", response.data);

      // 🔹 Transformar los datos del backend al formato esperado
      const userDocuments: FrontendDocument[] = response.data.map((doc: BackendDocument) => {
        return {
          id: doc.id.toString(),
          name: doc.nombreArchivo,
          type: determineDocumentType(doc.nombreArchivo),
          size: formatFileSize(doc),
          date: formatDate(doc.fechaSubida),
        };
      });

      setDocuments(userDocuments);
    } catch (error: unknown) {
      console.error("❌ Error al obtener documentos:", error);
      if (axios.isAxiosError(error)) {
        console.error("📊 Respuesta del error:", error.response?.data);
      }
    } finally {
      setLoading(false);
    }
  };

  // 🔹 Cargar documentos cuando la sesión esté disponible
  useEffect(() => {
    if (status === "authenticated" && session?.user?.id) {
      console.log(session, "Checho mensaje:");

      fetchUserDocuments();
    }
  }, [status, session]); // ✅ Agregué las dependencias correctas

  // 🔹 Subir archivo al backend
  const uploadToBackend = async (file: File) => {
    if (!session?.user?.id) {
      console.error("🚨 No hay sesión activa o usuarioId.");
      return;
    }

    const formData = new FormData();
    formData.append("Archivo", file);
    formData.append("UsuarioId", session.user.id);

    try {
      setUploading(true);

      const response = await axios.post(API_ROUTES.UPLOAD_DOCUMENT, formData, {
        headers: {
          "Content-Type": "multipart/form-data",
          Authorization: `Bearer ${session.accessToken}`,
        },
      });

      console.log("✅ Documento subido:", response.data);

      // 🔹 Recargar TODOS los documentos desde la BD después de subir
      await fetchUserDocuments();

    } catch (error: unknown) {
      console.error("❌ Error al subir documento:", error);
      if (axios.isAxiosError(error)) {
        console.error("📊 Detalles del error:", error.response?.data);
      }
    } finally {
      setUploading(false);
    }
  };

  // 🔹 Eliminar documento
  const handleRemove = async (documentId: string) => {
    if (!session) {
      console.error("🚨 No hay sesión activa.");
      return;
    }

    try {
      await axios.delete(`${API_ROUTES.DELETE_DOCUMENT}/${documentId}`, {
        headers: {
          Authorization: `Bearer ${session.accessToken}`,
        },
      });

      console.log("✅ Documento eliminado:", documentId);
      
      // 🔹 Recargar la lista desde la BD después de eliminar
      await fetchUserDocuments();
      
    } catch (error: unknown) {
      console.error("❌ Error al eliminar documento:", error);
      if (axios.isAxiosError(error)) {
        console.error("📊 Detalles del error:", error.response?.data);
      }
    }
  };

  // 🔹 Manejar arrastrar y soltar
  const handleDrop = async (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setDragActive(false);

    const files = Array.from(e.dataTransfer.files);
    for (const file of files) await uploadToBackend(file);
  };

  // 🔹 Manejar selección manual
  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    for (const file of files) await uploadToBackend(file);
  };

  // ✅ Verificación de sesión como en el dashboard
  if (status === 'loading') {
    return (
      <Layout>
        <div className="flex items-center justify-center min-h-screen">
          <p className="text-gray-600">Cargando sesión...</p>
        </div>
      </Layout>
    );
  }

  if (!session) {
    return (
      <Layout>
        <div className="flex items-center justify-center min-h-screen">
          <p className="text-gray-600">No estás autenticado.</p>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Gestión de Documentos</h1>
        <button
          onClick={fetchUserDocuments}
          className="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 text-sm"
        >
          Actualizar
        </button>
      </div>

      {/* File Upload Area */}
      <div
        className={`border-2 border-dashed rounded-xl p-8 mb-10 text-center transition ${
          dragActive
            ? "border-blue-600 bg-blue-50"
            : "border-gray-300 bg-white hover:bg-gray-50"
        }`}
        onDragOver={(e) => {
          e.preventDefault();
          setDragActive(true);
        }}
        onDragLeave={() => setDragActive(false)}
        onDrop={handleDrop}
      >
        <FaUpload className="text-4xl text-blue-600 mx-auto mb-3" />
        <p className="text-gray-700 font-medium mb-2">
          Arrastra tus documentos aquí o haz clic para subir
        </p>
        <p className="text-sm text-gray-500 mb-4">
          Formatos permitidos: PDF, Word, Excel
        </p>

        <input
          type="file"
          multiple
          className="hidden"
          id="fileInput"
          onChange={handleFileSelect}
          accept=".pdf,.doc,.docx,.xls,.xlsx"
        />
        <label
          htmlFor="fileInput"
          className="mt-4 inline-block px-5 py-2 bg-blue-600 text-white rounded-lg cursor-pointer hover:bg-blue-700"
        >
          {uploading ? "Subiendo..." : "Seleccionar archivos"}
        </label>
      </div>

      {/* Loading State */}
      {loading && (
        <div className="text-center py-8">
          <p className="text-gray-600">Cargando documentos...</p>
        </div>
      )}

      {/* Documents Grid */}
      {!loading && (
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {documents.length > 0 ? (
            documents.map((doc) => (
              <DocumentCard 
                key={doc.id} 
                {...doc} 
                onRemove={() => handleRemove(doc.id)} 
              />
            ))
          ) : (
            <div className="col-span-full text-center py-8">
              <p className="text-gray-600">No hay documentos subidos aún.</p>
              <p className="text-sm text-gray-500 mt-2">
                Sube tu primer documento usando el área de arriba
              </p>
            </div>
          )}
        </div>
      )}

      {/* Debug Info */}
      {process.env.NODE_ENV === 'development' && (
        <div className="mt-8 p-4 bg-gray-100 rounded-lg">
          <details>
            <summary className="cursor-pointer font-medium text-gray-700">
              Información de Debug (Solo Desarrollo)
            </summary>
            <div className="mt-2 text-sm text-gray-600">
              <p>Usuario ID: {session?.user?.id || "No disponible"}</p>
              <p>Total documentos: {documents.length}</p>
              <p>Estado sesión: {status}</p>
            </div>
          </details>
        </div>
      )}
    </Layout>
  );
}