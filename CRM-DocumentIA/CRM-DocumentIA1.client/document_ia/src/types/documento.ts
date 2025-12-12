// @/types/document.ts

// Interfaz para documentos del backend
export interface BackendDocument {
  id: number;
  usuarioId: number;
  nombreArchivo: string;
  tipoDocumento: string;
  rutaArchivo?: string;
  archivoDocumento?: number[]; // byte array
  tamañoArchivo: number;
  fechaSubida: string;
  estadoProcesamiento: "pendiente" | "procesando" | "completado" | "error";
  procesado: boolean;
  numeroImagenes?: number;
  resumenDocumento?: string;
  archivoMetadataJson?: string;
  errorProcesamiento?: string;
  fechaProcesamiento?: string;
  urlServicioIA?: string;
}

// Interfaz para documentos en el frontend
export interface Document {
  id: string;
  name: string;
  type: "PDF" | "Word" | "Excel";
  size: string;
  date: string;
}

// Tipos para documentos compartidos
export interface DocumentoCompartido {
  id: number;
  documentoId: number;
  nombreDocumento: string;
  usuarioPropietarioId: number;
  nombrePropietario: string;
  usuarioCompartidoId: number;
  nombreUsuarioCompartido: string;
  fechaCompartido: string;
  permiso: "lectura" | "escritura";
  mensaje?: string;
}

// Interfaz para usuario en búsqueda
export interface Usuario {
  id: number;
  nombre: string;
  email: string;
}

// DTO para compartir documento
export interface CompartirDocumentoRequest {
  documentoId: number;
  usuarioCompartidoId: number;
  permiso: "lectura" | "escritura";
  mensaje?: string;
}

// DTO para respuesta al compartir
export interface CompartirDocumentoResponse {
  mensaje: string;
  compartidoId: number;
  documentoId: number;
  usuarioCompartidoId: number;
  fecha: string;
}

// Respuesta de documentos compartidos conmigo
export interface DocumentosCompartidosResponse {
  total: number;
  documentos: DocumentoCompartido[];
}

// Respuesta de búsqueda de usuarios
export interface BuscarUsuariosResponse {
  total: number;
  usuarios: Usuario[];
}

// Respuesta de verificación de permiso
export interface VerificarPermisoResponse {
  tienePermiso: boolean;
}

// Tipo para estado de procesamiento
export type EstadoProcesamiento = "pendiente" | "procesando" | "completado" | "error";

// Tipo para permisos de documento
export type PermisoDocumento = "lectura" | "escritura";

// Tipo para formato de documento
export type TipoDocumento = "PDF" | "Word" | "Excel" | "Imagen" | "Texto" | "Otro";

// Interfaz para estadísticas de documentos
export interface DocumentStats {
  total: number;
  completados: number;
  pendientes: number;
  errores: number;
  tamañoTotal: number;
  ultimoDocumento?: string;
}

// Interfaz para proceso IA
export interface ProcesoIA {
  id: number;
  documentoId: number;
  tipoProcesamiento: string;
  estado: string;
  fechaInicio: string;
  fechaFin?: string;
  resultadoJson?: string;
  tiempoProcesamientoSegundos?: number;
  error?: string;
}

// Helper para determinar el tipo de documento
export const determinarTipoDocumento = (nombreArchivo: string): TipoDocumento => {
  const lowerNombre = nombreArchivo.toLowerCase();
  
  if (lowerNombre.endsWith('.pdf')) return 'PDF';
  if (lowerNombre.endsWith('.doc') || lowerNombre.endsWith('.docx')) return 'Word';
  if (lowerNombre.endsWith('.xls') || lowerNombre.endsWith('.xlsx')) return 'Excel';
  if (lowerNombre.endsWith('.jpg') || lowerNombre.endsWith('.jpeg') || 
      lowerNombre.endsWith('.png') || lowerNombre.endsWith('.gif')) return 'Imagen';
  if (lowerNombre.endsWith('.txt') || lowerNombre.endsWith('.md')) return 'Texto';
  
  return 'Otro';
};

// Helper para formatear tamaño de archivo
export const formatearTamañoArchivo = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes';
  
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
};

// Helper para convertir BackendDocument a Document
export const convertirBackendADocument = (backendDoc: BackendDocument): Document => {
  return {
    id: backendDoc.id.toString(),
    name: backendDoc.nombreArchivo,
    type: determinarTipoDocumento(backendDoc.nombreArchivo) as "PDF" | "Word" | "Excel",
    size: formatearTamañoArchivo(backendDoc.tamañoArchivo),
    date: new Date(backendDoc.fechaSubida).toLocaleDateString('es-ES', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    })
  };
};

// Helper para verificar si el usuario es propietario del documento
export const esPropietarioDocumento = (
  documento: BackendDocument | { usuarioId: number },
  usuarioId: number
): boolean => {
  return documento.usuarioId === usuarioId;
};

// Helper para obtener el icono según el tipo de documento
export const obtenerIconoDocumento = (tipo: TipoDocumento | "PDF" | "Word" | "Excel") => {
  const iconos = {
    PDF: '📄',
    Word: '📝',
    Excel: '📊',
    Imagen: '🖼️',
    Texto: '📃',
    Otro: '📎'
  };
  
  return iconos[tipo] || iconos.Otro;
};

// Helper para obtener color según estado de procesamiento
export const obtenerColorEstado = (estado: EstadoProcesamiento): string => {
  const colores = {
    pendiente: 'text-yellow-600 bg-yellow-100',
    procesando: 'text-blue-600 bg-blue-100',
    completado: 'text-green-600 bg-green-100',
    error: 'text-red-600 bg-red-100'
  };
  
  return colores[estado];
};

// Helper para formatear fecha
export const formatearFecha = (fechaString: string, incluirHora: boolean = false): string => {
  const fecha = new Date(fechaString);
  const opciones: Intl.DateTimeFormatOptions = {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  };
  
  if (incluirHora) {
    opciones.hour = '2-digit';
    opciones.minute = '2-digit';
  }
  
  return fecha.toLocaleDateString('es-ES', opciones) + 
    (incluirHora ? ` ${fecha.getHours().toString().padStart(2, '0')}:${fecha.getMinutes().toString().padStart(2, '0')}` : '');
};