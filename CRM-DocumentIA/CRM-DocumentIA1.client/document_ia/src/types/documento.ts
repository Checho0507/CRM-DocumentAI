// Crea este archivo: @/types/document.ts

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
}

// Y tu interfaz frontend existente
export interface Document {
  id: string;
  name: string;
  type: "PDF" | "Word" | "Excel";
  size: string;
  date: string;
}