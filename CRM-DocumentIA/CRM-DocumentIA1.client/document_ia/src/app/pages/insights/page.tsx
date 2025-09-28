"use client";

import Layout from "@/components/Layout/Layout";
import { useState } from "react";
import { FaSearch, FaBell, FaChevronDown, FaPaperPlane, FaTimes, FaHistory, FaQuestionCircle, FaCopy, FaThumbsUp, FaThumbsDown, FaEye, FaTrash } from "react-icons/fa";

export default function InsightsPage() {
  const [query, setQuery] = useState("");
  const [response, setResponse] = useState<string | null>(null);
  const [sources, setSources] = useState<{ name: string; type: string; date: string; relevance: string }[]>([]);

  const exampleQuestions = [
    {
      question: "¿Qué dice el contrato sobre cláusula de terminación?",
      answer: "Según los documentos analizados, la cláusula de terminación (sección 9.3) establece que cualquier parte puede terminar este acuerdo con 30 días de notificación por escrito...",
      sources: [
        { name: "Contrato_Master_Servicios.pdf", type: "pdf", date: "15/08/2023", relevance: "95%" },
        { name: "Anexo_Términos_Condiciones.docx", type: "word", date: "20/07/2023", relevance: "87%" },
      ],
    },
  ];

  const handleAsk = () => {
    if (!query.trim()) return;
    const res = exampleQuestions[0];
    setResponse(res.answer);
    setSources(res.sources);
  };

  return (
    <Layout>
      {/* Header */}

      {/* Title */}
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Insights - Consultas RAG</h1>
        <div className="flex items-center gap-2 bg-white px-4 py-2 rounded-full shadow">
          <i className="fas fa-calendar text-gray-500" />
          <span>Septiembre 2023</span>
          <FaChevronDown className="text-gray-500" />
        </div>
      </div>

      {/* Query Section */}
      <div className="bg-white p-6 rounded-xl shadow mb-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold">Consulta de Documentos</h2>
          <div className="flex gap-2">
            <button className="px-3 py-1 text-sm bg-gray-100 rounded-md flex items-center gap-1">
              <FaHistory /> Historial
            </button>
            <button className="px-3 py-1 text-sm bg-gray-100 rounded-md flex items-center gap-1">
              <FaQuestionCircle /> Ayuda
            </button>
          </div>
        </div>

        <textarea
          className="w-full border border-gray-300 rounded-xl p-3 min-h-[100px] mb-4 focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="Escribe tu pregunta sobre los documentos..."
          value={query}
          onChange={(e) => setQuery(e.target.value)}
        />

        <div className="flex justify-end gap-2">
          <button
            className="px-4 py-2 border border-blue-600 text-blue-600 rounded-md flex items-center gap-2"
            onClick={() => {
              setQuery("");
              setResponse(null);
              setSources([]);
            }}
          >
            <FaTimes /> Limpiar
          </button>
          <button
            className="px-4 py-2 bg-blue-600 text-white rounded-md flex items-center gap-2"
            onClick={handleAsk}
          >
            <FaPaperPlane /> Enviar consulta
          </button>
        </div>

        {/* Response */}
        {response && (
          <div className="mt-6 border-t border-gray-200 pt-4">
            <div className="flex items-center justify-between mb-2">
              <h3 className="font-semibold">Respuesta</h3>
              <div className="flex gap-2">
                <button className="text-sm px-2 py-1 bg-gray-100 rounded-md flex items-center gap-1">
                  <FaCopy /> Copiar
                </button>
                <button className="text-sm px-2 py-1 bg-gray-100 rounded-md">
                  <FaThumbsUp />
                </button>
                <button className="text-sm px-2 py-1 bg-gray-100 rounded-md">
                  <FaThumbsDown />
                </button>
              </div>
            </div>
            <div className="bg-gray-100 p-3 rounded-lg mb-4">{response}</div>

            <h3 className="font-semibold mb-2">Fuentes</h3>
            <div className="grid gap-2">
              {sources.map((src, i) => (
                <div
                  key={i}
                  className="flex items-center justify-between p-3 bg-white border rounded-lg"
                >
                  <div className="flex items-center gap-3">
                    <div className="w-8 h-8 bg-blue-600 text-white flex items-center justify-center rounded-md">
                      <i className={`fas fa-file-${src.type === "pdf" ? "pdf" : "word"}`} />
                    </div>
                    <div>
                      <p className="font-medium">{src.name}</p>
                      <p className="text-xs text-gray-500">
                        Fecha: {src.date} • Relevancia: {src.relevance}
                      </p>
                    </div>
                  </div>
                  <button className="px-2 py-1 text-sm bg-gray-100 rounded-md">
                    <FaEye />
                  </button>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* History Section */}
      <div className="bg-white p-6 rounded-xl shadow">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold">Consultas Recientes</h2>
          <button className="px-3 py-1 text-sm border border-blue-600 text-blue-600 rounded-md flex items-center gap-1">
            <FaTrash /> Limpiar historial
          </button>
        </div>

        <div className="grid gap-3">
          <div className="p-4 bg-gray-100 rounded-lg cursor-pointer hover:bg-blue-50">
            <p className="font-medium flex items-center gap-2">
              <FaQuestionCircle className="text-blue-600" /> ¿Qué dice el contrato con Empresa XYZ sobre renovación automática?
            </p>
            <p className="text-sm text-gray-600 mt-1 line-clamp-2">
              El contrato con Empresa XYZ establece en la cláusula 8.2 que el acuerdo se renovará automáticamente...
            </p>
            <p className="text-xs text-gray-500 mt-2">Hace 2 días • 3 fuentes</p>
          </div>
          <div className="p-4 bg-gray-100 rounded-lg cursor-pointer hover:bg-blue-50">
            <p className="font-medium flex items-center gap-2">
              <FaQuestionCircle className="text-blue-600" /> ¿Cuáles son los términos de pago con Tech Solutions?
            </p>
            <p className="text-sm text-gray-600 mt-1 line-clamp-2">
              Los términos de pago con Tech Solutions establecen un neto 30 desde la fecha de facturación...
            </p>
            <p className="text-xs text-gray-500 mt-2">Hace 5 días • 2 fuentes</p>
          </div>
        </div>
      </div>
    </Layout>
  );
}
