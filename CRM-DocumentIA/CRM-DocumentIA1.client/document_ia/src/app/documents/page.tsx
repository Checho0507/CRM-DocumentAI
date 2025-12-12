"use client";

import { useState, useEffect } from "react";
import Layout from "@/components/Layout/Layout";
import { FaUpload, FaFilePdf, FaFileWord, FaFileExcel } from "react-icons/fa";
import DocumentCard from "@/components/Common/DocumentCard";
import ShareDocumentModal from "@/components/Documents/SharedDocumentModal";
import SharedDocumentsPanel from "@/components/Documents/SharedDocumentsPanel";
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
  originalData: BackendDocument;
}

export default function DocumentsPage() {
  const { data: session, status } = useSession();
  const [documents, setDocuments] = useState<FrontendDocument[]>([]);
  const [selectedDoc, setSelectedDoc] = useState<string | null>(null);
  const [dragActive, setDragActive] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  
  // Estados para compartir documentos
  const [isShareModalOpen, setIsShareModalOpen] = useState(false);
  const [selectedDocumentForShare, setSelectedDocumentForShare] = useState<{
    id: string;
    name: string;
  } | null>(null);

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
      if (doc.tamañoArchivo < 1024 * 1024) {
        return `${(doc.tamañoArchivo / 1024).toFixed(1)} KB`;
      }
      return `${(doc.tamañoArchivo / (1024 * 1024)).toFixed(1)} MB`;
    }
    return "0 KB";
  };

  // 🔹 Formatear la fecha
  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return date.toLocaleDateString("es-ES", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    });
  };

  // 🔹 Obtener documentos del usuario desde la BD
  const fetchUserDocuments = async () => {
    if (!session?.user?.id) {
      return;
    }

    try {
      setLoading(true);
      setError(null);
      
      // ✅ CORREGIDO: Usar API_ROUTES.GET_USER_DOCUMENTS como URL base y agregar el ID del usuario
      const response = await axios.get<BackendDocument[]>(
        `${API_ROUTES.GET_USER_DOCUMENTS}${session.user.id}`,
        {
          headers: {
            Authorization: `Bearer ${session.accessToken}`,
          },
        }
      );
      
      // 🔹 Transformar los datos del backend al formato esperado
      const userDocuments: FrontendDocument[] = response.data.map((doc: BackendDocument) => {
        return {
          id: doc.id.toString(),
          name: doc.nombreArchivo,
          type: determineDocumentType(doc.nombreArchivo),
          size: formatFileSize(doc),
          date: formatDate(doc.fechaSubida),
          originalData: doc,
        };
      });

      setDocuments(userDocuments);
      setSuccessMessage(`${userDocuments.length} documentos cargados exitosamente`);
      setTimeout(() => setSuccessMessage(null), 3000);
      
    } catch (error: unknown) {
      console.error("❌ Error al obtener documentos:", error);
      if (axios.isAxiosError(error)) {
        console.error("📊 Respuesta del error:", error.response?.data);
      }
    } finally {
      setLoading(false);
    }
  };

  // 🔹 Subir archivo al backend
  const uploadToBackend = async (file: File) => {
    if (!session?.user?.id) {
      setError("No hay sesión activa. Por favor, inicia sesión.");
      return;
    }

    // Validar tamaño del archivo (máximo 50MB)
    if (file.size > 50 * 1024 * 1024) {
      setError("El archivo es demasiado grande. Máximo 50MB.");
      return;
    }

    // Validar tipo de archivo
    const validTypes = [".pdf", ".docx", ".xls", ".xlsx"];
    const fileExtension = file.name.toLowerCase().slice(file.name.lastIndexOf("."));
    if (!validTypes.includes(fileExtension)) {
      setError("Tipo de archivo no válido. Solo se permiten PDF, Word y Excel.");
      return;
    }

    const formData = new FormData();
    formData.append("Archivo", file);
    formData.append("UsuarioId", session.user.id);

    try {
      setUploading(true);
      setError(null);

      const response = await axios.post(API_ROUTES.UPLOAD_DOCUMENT, formData, {
        headers: {
          Authorization: `Bearer ${session.accessToken}`,
        },
        onUploadProgress: (progressEvent) => {
          if (progressEvent.total) {
            const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
            console.log(`Upload progress: ${percentCompleted}%`);
          }
        },
      });

      console.log("✅ Documento subido:", response.data);
      setSuccessMessage(`"${file.name}" subido exitosamente`);
      
      // 🔹 Recargar TODOS los documentos desde la BD después de subir
      await fetchUserDocuments();

    } catch (error: unknown) {
      console.error("❌ Error al subir documento:", error);
      setError("Error al subir el documento. Por favor, intenta nuevamente.");
      if (axios.isAxiosError(error)) {
        console.error("📊 Detalles del error:", error.response?.data);
      }
    } finally {
      setUploading(false);
    }
  };

  // 🔹 Eliminar documento
  const handleRemove = async (documentId: string, documentName: string) => {
    if (!session) {
      setError("No hay sesión activa.");
      return;
    }

    if (!confirm(`¿Estás seguro de que deseas eliminar "${documentName}"?`)) {
      return;
    }

    try {
      // ✅ CORREGIDO: Usar API_ROUTES.DELETE_DOCUMENT como función
      await axios.delete(API_ROUTES.DELETE_DOCUMENT(documentId), {
        headers: {
          Authorization: `Bearer ${session.accessToken}`,
        },
      });

      console.log("✅ Documento eliminado:", documentId);
      setSuccessMessage(`"${documentName}" eliminado exitosamente`);
      
      // 🔹 Recargar la lista desde la BD después de eliminar
      await fetchUserDocuments();
      
    } catch (error: unknown) {
      console.error("❌ Error al eliminar documento:", error);
      setError("Error al eliminar el documento. Por favor, intenta nuevamente.");
      if (axios.isAxiosError(error)) {
        console.error("📊 Detalles del error:", error.response?.data);
      }
    }
  };

  // 🔹 Ver documento
  const handleView = (documentId: string, documentName: string) => {
    console.log(`Ver documento: ${documentName} (ID: ${documentId})`);
    // Aquí podrías implementar la lógica para ver el documento
    // Por ejemplo: abrir en una nueva pestaña, modal, etc.
    alert(`Funcionalidad de vista para "${documentName}" en desarrollo.`);
  };

  // 🔹 Descargar documento
  const handleDownload = async (documentId: string, documentName: string) => {
    try {
      // ✅ CORREGIDO: Usar API_ROUTES.DOWNLOAD_DOCUMENT como función
      const response = await axios.get(API_ROUTES.DOWNLOAD_DOCUMENT(documentId), {
        headers: {
          Authorization: `Bearer ${session?.accessToken}`,
        },
        responseType: 'blob',
      });

      // Crear enlace de descarga
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', documentName);
      document.body.appendChild(link);
      link.click();
      
      // Limpiar recursos
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      
      // Mostrar mensaje de éxito
      setSuccessMessage(`"${documentName}" descargado exitosamente`);
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (error) {
      console.error("Error al descargar documento:", error);
      setError("Error al descargar el documento.");
    }
  };

  // 🔹 Función para abrir modal de compartir
  const openShareModal = (documentId: string, documentName: string) => {
    setSelectedDocumentForShare({ id: documentId, name: documentName });
    setIsShareModalOpen(true);
  };

  // 🔹 Función para cerrar modal de compartir
  const handleShareModalClose = () => {
    setIsShareModalOpen(false);
    setSelectedDocumentForShare(null);
  };

  // 🔹 Manejar arrastrar y soltar
  const handleDrop = async (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setDragActive(false);

    const files = Array.from(e.dataTransfer.files);
    for (const file of files) {
      await uploadToBackend(file);
    }
  };

  // 🔹 Manejar selección manual
  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    for (const file of files) {
      await uploadToBackend(file);
    }
    // Resetear input para permitir subir el mismo archivo otra vez
    e.target.value = "";
  };

  // 🔹 Cargar documentos cuando la sesión esté disponible
  useEffect(() => {
    if (status === "authenticated" && session?.user?.id) {
      fetchUserDocuments();
    }
  }, [status, session]);

  // 🔹 Mensajes temporales
  useEffect(() => {
    if (successMessage || error) {
      const timer = setTimeout(() => {
        setSuccessMessage(null);
        setError(null);
      }, 5000);
      return () => clearTimeout(timer);
    }
  }, [successMessage, error]);

  // ✅ Verificación de sesión
  if (status === "loading") {
    return (
      <Layout>
        <div className="flex items-center justify-center min-h-screen">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </Layout>
    );
  }

  if (!session) {
    return (
      <Layout>
        <div className="flex items-center justify-center min-h-screen">
          <div className="text-center">
            <h2 className="text-2xl font-bold text-gray-800 mb-4">Acceso no autorizado</h2>
            <p className="text-gray-600">Debes iniciar sesión para acceder a esta página.</p>
          </div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-800 mb-2">Gestión de Documentos</h1>
        <p className="text-gray-600">
          Sube, visualiza y gestiona tus documentos en un solo lugar.
        </p>
      </div>

      {/* Mensajes de estado */}
      {successMessage && (
        <div className="mb-6 p-4 bg-green-100 text-green-700 rounded-lg border border-green-200">
          ✅ {successMessage}
        </div>
      )}

      {error && (
        <div className="mb-6 p-4 bg-red-100 text-red-700 rounded-lg border border-red-200">
          ❌ {error}
        </div>
      )}

      {/* Estadísticas rápidas */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
        <div className="bg-white p-4 rounded-lg shadow">
          <div className="flex items-center gap-3">
            <FaFilePdf className="text-red-500 text-2xl" />
            <div>
              <p className="text-2xl font-bold text-gray-800">
                {documents.filter(d => d.type === "PDF").length}
              </p>
              <p className="text-sm text-gray-500">Documentos PDF</p>
            </div>
          </div>
        </div>
        <div className="bg-white p-4 rounded-lg shadow">
          <div className="flex items-center gap-3">
            <FaFileWord className="text-blue-500 text-2xl" />
            <div>
              <p className="text-2xl font-bold text-gray-800">
                {documents.filter(d => d.type === "Word").length}
              </p>
              <p className="text-sm text-gray-500">Documentos Word</p>
            </div>
          </div>
        </div>
        <div className="bg-white p-4 rounded-lg shadow">
          <div className="flex items-center gap-3">
            <FaFileExcel className="text-green-500 text-2xl" />
            <div>
              <p className="text-2xl font-bold text-gray-800">
                {documents.filter(d => d.type === "Excel").length}
              </p>
              <p className="text-sm text-gray-500">Documentos Excel</p>
            </div>
          </div>
        </div>
        <div className="bg-white p-4 rounded-lg shadow">
          <div className="flex items-center gap-3">
            <FaUpload className="text-purple-500 text-2xl" />
            <div>
              <p className="text-2xl font-bold text-gray-800">{documents.length}</p>
              <p className="text-sm text-gray-500">Total documentos</p>
            </div>
          </div>
        </div>
      </div>

      {/* File Upload Area */}
      <div
        className={`border-2 border-dashed rounded-xl p-8 mb-10 text-center transition-all duration-300 ${
          dragActive
            ? "border-blue-600 bg-blue-50 scale-[1.01]"
            : "border-gray-300 bg-white hover:bg-gray-50"
        }`}
        onDragOver={(e) => {
          e.preventDefault();
          setDragActive(true);
        }}
        onDragLeave={() => setDragActive(false)}
        onDrop={handleDrop}
      >
        <div className="max-w-md mx-auto">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-100 rounded-full mb-4">
            <FaUpload className="text-2xl text-blue-600" />
          </div>
          <h3 className="text-lg font-semibold text-gray-800 mb-2">
            Arrastra y suelta tus archivos aquí
          </h3>
          <p className="text-sm text-gray-600 mb-6">
            O haz clic para seleccionar archivos de tu computadora
          </p>
          <p className="text-xs text-gray-500 mb-6">
            Formatos soportados: PDF (.pdf), Word (.doc, .docx), Excel (.xls, .xlsx)
            <br />
            Tamaño máximo: 50MB por archivo
          </p>

          <input
            type="file"
            multiple
            className="hidden"
            id="fileInput"
            onChange={handleFileSelect}
            accept=".pdf,.doc,.docx,.xls,.xlsx"
            disabled={uploading}
          />
          <label
            htmlFor="fileInput"
            className={`inline-flex items-center gap-2 px-6 py-3 rounded-lg cursor-pointer transition ${
              uploading
                ? "bg-gray-400 text-white cursor-not-allowed"
                : "bg-blue-600 text-white hover:bg-blue-700"
            }`}
          >
            {uploading ? (
              <>
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                Subiendo...
              </>
            ) : (
              <>
                <FaUpload />
                Seleccionar archivos
              </>
            )}
          </label>
        </div>
      </div>

      {/* Controles y filtros */}
      <div className="flex flex-col sm:flex-row justify-between items-center mb-6 gap-4">
        <h2 className="text-xl font-semibold text-gray-800">
          Tus documentos ({documents.length})
        </h2>
        <div className="flex gap-3">
          <button
            onClick={fetchUserDocuments}
            disabled={loading}
            className="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 text-sm flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loading ? "Actualizando..." : "Actualizar lista"}
          </button>
          {selectedDoc && (
            <button
              onClick={() => setSelectedDoc(null)}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 text-sm"
            >
              Deseleccionar
            </button>
          )}
        </div>
      </div>

      {/* Loading State */}
      {loading && documents.length === 0 ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Cargando tus documentos...</p>
        </div>
      ) : null}

      {/* Documents Grid */}
      {!loading || documents.length > 0 ? (
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {documents.length > 0 ? (
            documents.map((doc) => (
              <DocumentCard 
                key={doc.id} 
                name={doc.name}
                type={doc.type}
                size={doc.size}
                date={doc.date}
                onRemove={() => handleRemove(doc.id, doc.name)}
                onView={() => handleView(doc.id, doc.name)}
                onDownload={() => handleDownload(doc.id, doc.name)}
                onShare={() => openShareModal(doc.id, doc.name)}
                isSelected={selectedDoc === doc.id}
                onSelect={() => setSelectedDoc(doc.id === selectedDoc ? null : doc.id)}
              />
            ))
          ) : (
            <div className="col-span-full text-center py-12">
              <div className="inline-flex items-center justify-center w-16 h-16 bg-gray-100 rounded-full mb-4">
                <FaFilePdf className="text-2xl text-gray-400" />
              </div>
              <h3 className="text-lg font-medium text-gray-700 mb-2">
                No hay documentos
              </h3>
              <p className="text-gray-500 max-w-md mx-auto">
                Sube tu primer documento arrastrándolo al área de arriba o haciendo clic en Seleccionar archivos.
              </p>
            </div>
          )}
        </div>
      ) : null}

      {/* Panel de documentos compartidos */}
      <SharedDocumentsPanel />

      {/* Panel de información del documento seleccionado */}
      {selectedDoc && (
        <div className="mt-8 p-6 bg-white rounded-xl shadow">
          <h3 className="text-lg font-semibold text-gray-800 mb-4">
            Documento seleccionado
          </h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <p className="text-sm text-gray-500">ID del documento</p>
              <p className="font-mono text-gray-800">{selectedDoc}</p>
            </div>
            <div>
              <p className="text-sm text-gray-500">Acciones disponibles</p>
              <div className="flex gap-2 mt-2">
                <button className="px-3 py-1 bg-blue-100 text-blue-700 text-sm rounded hover:bg-blue-200">
                  Procesar
                </button>
                <button 
                  className="px-3 py-1 bg-green-100 text-green-700 text-sm rounded hover:bg-green-200"
                  onClick={() => {
                    const doc = documents.find(d => d.id === selectedDoc);
                    if (doc) {
                      openShareModal(doc.id, doc.name);
                    }
                  }}
                >
                  Compartir
                </button>
                <button className="px-3 py-1 bg-yellow-100 text-yellow-700 text-sm rounded hover:bg-yellow-200">
                  Renombrar
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Modal para compartir documento */}
      {isShareModalOpen && selectedDocumentForShare && (
        <ShareDocumentModal
          isOpen={isShareModalOpen}
          onClose={handleShareModalClose}
          documentId={selectedDocumentForShare.id}
          documentName={selectedDocumentForShare.name}
          onSuccess={() => {
            handleShareModalClose();
          }}
        />
      )}

      {/* Información de debug (solo desarrollo) */}
      {process.env.NODE_ENV === "development" && (
        <div className="mt-8 p-4 bg-gray-100 rounded-lg">
          <details className="cursor-pointer">
            <summary className="font-medium text-gray-700">
              Información de depuración
            </summary>
            <div className="mt-3 space-y-2 text-sm text-gray-600">
              <div className="grid grid-cols-2 gap-2">
                <div>
                  <span className="font-medium">Usuario ID:</span>{" "}
                  {session?.user?.id || "N/A"}
                </div>
                <div>
                  <span className="font-medium">Estado:</span> {status}
                </div>
                <div>
                  <span className="font-medium">Total documentos:</span>{" "}
                  {documents.length}
                </div>
                <div>
                  <span className="font-medium">Subiendo:</span>{" "}
                  {uploading ? "Sí" : "No"}
                </div>
              </div>
              <div className="pt-2 border-t border-gray-300">
                <span className="font-medium">Documentos:</span>
                <pre className="mt-1 p-2 bg-gray-800 text-gray-100 rounded text-xs overflow-auto max-h-40">
                  {JSON.stringify(documents, null, 2)}
                </pre>
              </div>
            </div>
          </details>
        </div>
      )}
    </Layout>
  );
}