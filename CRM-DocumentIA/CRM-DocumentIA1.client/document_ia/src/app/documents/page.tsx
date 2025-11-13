"use client";

import { useState } from "react";
import Layout from "@/components/Layout/Layout";
import { FaUpload } from "react-icons/fa";
import DocumentCard from "@/components/Common/DocumentCard";
import axios from "axios";
import { API_ROUTES } from "@/lib/apiRoutes";
import { useSession } from "next-auth/react";

export default function DocumentsPage() {
  interface Document {
    name: string;
    type: "PDF" | "Word" | "Excel";
    size: string;
    date: string;
  }

  const { data: session, status } = useSession(); // ✅ obtener sesión global
  const [documents, setDocuments] = useState<Document[]>([]);
  const [dragActive, setDragActive] = useState(false);
  const [uploading, setUploading] = useState(false);

  // 🔹 Subir archivo al backend con usuarioId y token
  const uploadToBackend = async (file: File) => {
    if (status !== "authenticated" || !session?.user?.id) {
      console.error("🚨 No hay sesión activa o usuarioId.");
      return;
    }

    const formData = new FormData();
    formData.append("archivo", file);
    formData.append("usuarioId", session.user.id); // 👈 se agrega el usuarioId

    try {
      setUploading(true);

      const response = await axios.post(API_ROUTES.UPLOAD_DOCUMENT, formData, {
        headers: {
          "Content-Type": "multipart/form-data",
          Authorization: `Bearer ${session.accessToken}`, // 👈 incluye el token JWT
        },
      });

      console.log("✅ Documento subido:", response.data);

      const newDoc: Document = {
        name: file.name,
        type: file.name.endsWith(".pdf")
          ? "PDF"
          : file.name.endsWith(".docx")
          ? "Word"
          : "Excel",
        size: `${(file.size / (1024 * 1024)).toFixed(1)} MB`,
        date: new Date().toISOString().split("T")[0],
      };

      setDocuments((prev) => [newDoc, ...prev]);
    } catch (error: unknown) {
      console.error("❌ Error al subir documento:", error);
    } finally {
      setUploading(false);
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

  const handleRemove = (name: string) => {
    setDocuments((prev) => prev.filter((doc) => doc.name !== name));
  };

  return (
    <Layout>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Gestión de Documentos</h1>
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
        />
        <label
          htmlFor="fileInput"
          className="mt-4 inline-block px-5 py-2 bg-blue-600 text-white rounded-lg cursor-pointer hover:bg-blue-700"
        >
          {uploading ? "Subiendo..." : "Seleccionar archivos"}
        </label>
      </div>

      {/* Documents Grid */}
      <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
        {documents.map((doc, idx) => (
          <DocumentCard key={idx} {...doc} onRemove={() => handleRemove(doc.name)} />
        ))}
      </div>
    </Layout>
  );
}
