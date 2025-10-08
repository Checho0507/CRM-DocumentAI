"use client";

import Layout from "@/components/Layout/Layout";
import ClientCard from "@/components/Common/ClientCard";
import { FaPlus } from "react-icons/fa";

export default function ClientsPage() {
  const clients = [
    {
      initials: "EA",
      name: "Empresa ABC",
      since: "Feb 2021",
      stats: [
        { value: "24", label: "Contratos" },
        { value: "78%", label: "Éxito" },
        { value: "3.8/5", label: "Sentimiento" },
        { value: "Medio", label: "Riesgo" },
      ],
    },
    {
      initials: "CZ",
      name: "Corporación XYZ",
      since: "Jun 2022",
      stats: [
        { value: "12", label: "Contratos" },
        { value: "92%", label: "Éxito" },
        { value: "4.5/5", label: "Sentimiento" },
        { value: "Bajo", label: "Riesgo" },
      ],
    },
    {
      initials: "TS",
      name: "Tech Solutions",
      since: "Nov 2020",
      stats: [
        { value: "37", label: "Contratos" },
        { value: "65%", label: "Éxito" },
        { value: "3.2/5", label: "Sentimiento" },
        { value: "Alto", label: "Riesgo" },
      ],
    },
    {
      initials: "GT",
      name: "Global Tech",
      since: "Mar 2023",
      stats: [
        { value: "5", label: "Contratos" },
        { value: "100%", label: "Éxito" },
        { value: "4.8/5", label: "Sentimiento" },
        { value: "Bajo", label: "Riesgo" },
      ],
    },
    {
      initials: "SI",
      name: "Servicios Integrales",
      since: "Sep 2022",
      stats: [
        { value: "15", label: "Contratos" },
        { value: "86%", label: "Éxito" },
        { value: "4.1/5", label: "Sentimiento" },
        { value: "Medio", label: "Riesgo" },
      ],
    },
  ];

  return (
    <Layout>
      {/* Título */}
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Gestión de Clientes</h1>
        <div className="flex items-center gap-2 bg-white shadow px-4 py-2 rounded-full cursor-pointer">
          <FaPlus className="text-gray-600" />
          <span>Nuevo cliente</span>
        </div>
      </div>

      {/* Grid de clientes */}
      <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
        {clients.map((client, i) => (
          <ClientCard key={i} {...client} />
        ))}
      </div>
    </Layout>
  );
}
