// @/lib/documentHelpers.ts
import { BackendDocument, Document, TipoDocumento, EstadoProcesamiento } from '../types/documento';

export const documentHelpers = {
  // Determinar tipo de documento basado en extensión
  determineDocumentType: (fileName: string): TipoDocumento => {
    const lowerFileName = fileName.toLowerCase();
    
    if (lowerFileName.endsWith(".pdf")) return "PDF";
    if (lowerFileName.endsWith(".docx") || lowerFileName.endsWith(".doc")) return "Word";
    if (lowerFileName.endsWith(".xlsx") || lowerFileName.endsWith(".xls")) return "Excel";
    if (lowerFileName.endsWith(".jpg") || lowerFileName.endsWith(".jpeg") || 
        lowerFileName.endsWith(".png") || lowerFileName.endsWith(".gif")) return "Imagen";
    if (lowerFileName.endsWith(".txt")) return "Texto";
    return "Otro";
  },

  // Formatear tamaño de archivo
  formatFileSize: (bytes: number): string => {
    if (bytes === 0) return "0 Bytes";
    
    const k = 1024;
    const sizes = ["Bytes", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  },

  // Convertir BackendDocument a Document
  convertBackendToFrontend: (backendDoc: BackendDocument): Document => {
    return {
      id: backendDoc.id.toString(),
      name: backendDoc.nombreArchivo,
      type: documentHelpers.determineDocumentType(backendDoc.nombreArchivo) as "PDF" | "Word" | "Excel",
      size: documentHelpers.formatFileSize(backendDoc.tamañoArchivo),
      date: new Date(backendDoc.fechaSubida).toISOString().split('T')[0]
    };
  },

  // Formatear fecha
  formatDate: (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('es-ES', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  },

  // Obtener color para estado
  getStatusColor: (estado: EstadoProcesamiento): string => {
    const colors = {
      pendiente: "bg-yellow-100 text-yellow-800",
      procesando: "bg-blue-100 text-blue-800",
      completado: "bg-green-100 text-green-800",
      error: "bg-red-100 text-red-800"
    };
    return colors[estado];
  },

  // Obtener icono para tipo de documento
  getDocumentIcon: (type: TipoDocumento | "PDF" | "Word" | "Excel"): string => {
    const icons = {
      PDF: "📄",
      Word: "📝",
      Excel: "📊",
      Imagen: "🖼️",
      Texto: "📃",
      Otro: "📎"
    };
    return icons[type] || icons.Otro;
  }
};