"use client";

import { useState } from "react";
import Layout from "@/components/Layout/Layout";
import { FaUpload } from "react-icons/fa";
import DocumentCard from "@/components/Common/DocumentCard";

export default function DocumentsPage() {
  interface Document {
    name: string;
    type: "PDF" | "Word" | "Excel";
    size: string;
    date: string;
  }

  const [documents, setDocuments] = useState<Document[]>([
    { name: "Contrato_Servicios_ABC.pdf", type: "PDF", size: "1.2 MB", date: "2023-10-01" },
    { name: "Propuesta_Corporación_XYZ.docx", type: "Word", size: "890 KB", date: "2023-09-18" },
    { name: "Informe_Financiero_Q3.xlsx", type: "Excel", size: "2.1 MB", date: "2023-09-05" },
    { name: "Cláusulas_Actualizadas_2023.pdf", type: "PDF", size: "640 KB", date: "2023-08-27" },
  ]);

  const [dragActive, setDragActive] = useState(false);

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setDragActive(false);

    const files = Array.from(e.dataTransfer.files);
    const newDocs = files.map((file) => ({
      name: file.name,
      type:
        file.name.endsWith(".pdf")
          ? "PDF"
          : file.name.endsWith(".docx")
          ? "Word"
          : "Excel",
      size: `${(file.size / (1024 * 1024)).toFixed(1)} MB`,
      date: new Date().toISOString().split("T")[0],
    }));

    setDocuments((prev) => [...newDocs, ...prev]);
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
        <p className="text-sm text-gray-500">Formatos permitidos: PDF, Word, Excel</p>
        <input
          type="file"
          multiple
          className="hidden"
          id="fileInput"
          onChange={(e) => {
            const files = Array.from(e.target.files || []);
            const newDocs = files.map((file) => ({
              name: file.name,
              type:
                file.name.endsWith(".pdf")
                  ? "PDF"
                  : file.name.endsWith(".docx")
                  ? "Word"
                  : "Excel",
              size: `${(file.size / (1024 * 1024)).toFixed(1)} MB`,
              date: new Date().toISOString().split("T")[0],
            }));
            setDocuments((prev) => [...newDocs, ...prev]);
          }}
        />
        <label
          htmlFor="fileInput"
          className="mt-4 inline-block px-5 py-2 bg-blue-600 text-white rounded-lg cursor-pointer hover:bg-blue-700"
        >
          Seleccionar archivos
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
