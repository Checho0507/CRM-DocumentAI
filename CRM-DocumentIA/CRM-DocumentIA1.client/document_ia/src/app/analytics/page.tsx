"use client";

import Layout from "@/components/Layout/Layout";
import Card from "@/components/UI/Card";
import { Line, Pie, Bar, Radar } from "react-chartjs-2";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  ArcElement,
  BarElement,
  RadialLinearScale,
  Title,
  Tooltip,
  Legend,
} from "chart.js";

// registrar módulos de Chart.js
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  ArcElement,
  BarElement,
  RadialLinearScale,
  Title,
  Tooltip,
  Legend
);

export default function AnalyticsPage() {
  // Datos de prueba
  const successData = {
    labels: ["Abr", "May", "Jun", "Jul", "Ago", "Sep"],
    datasets: [
      {
        label: "Tasa de éxito (%)",
        data: [72, 75, 78, 76, 80, 82],
        backgroundColor: "rgba(76,201,240,0.2)",
        borderColor: "#4cc9f0",
        borderWidth: 2,
        tension: 0.3,
        fill: true,
      },
    ],
  };

  const typeData = {
    labels: ["Contratos", "Propuestas", "Correos", "Informes"],
    datasets: [
      {
        data: [35, 25, 30, 10],
        backgroundColor: ["#4361ee", "#4cc9f0", "#f8961e", "#f72585"],
      },
    ],
  };

  const sentimentData = {
    labels: ["Abr", "May", "Jun", "Jul", "Ago", "Sep"],
    datasets: [
      { label: "Positivo", data: [65, 70, 75, 72, 78, 80], backgroundColor: "#4cc9f0" },
      { label: "Neutral", data: [25, 20, 15, 18, 14, 12], backgroundColor: "#f8961e" },
      { label: "Negativo", data: [10, 10, 10, 10, 8, 8], backgroundColor: "#f72585" },
    ],
  };

  const clientSuccessData = {
    labels: ["Empresa ABC", "Corporación XYZ", "Tech Solutions", "Global Tech", "Servicios Integrales"],
    datasets: [
      {
        label: "Tasa de éxito (%)",
        data: [78, 92, 65, 100, 86],
        backgroundColor: "rgba(76,201,240,0.2)",
        borderColor: "#4cc9f0",
        borderWidth: 2,
        pointBackgroundColor: "#4cc9f0",
      },
    ],
  };

  return (
    <Layout>
      {/* Título + filtro */}
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Analítica Avanzada</h1>
        <div className="flex items-center gap-2 bg-white shadow px-4 py-2 rounded-full">
          <i className="fas fa-calendar text-gray-500" />
          <span>Últimos 6 meses</span>
          <i className="fas fa-chevron-down text-gray-500" />
        </div>
      </div>

      {/* Filtros */}
      <div className="grid md:grid-cols-4 gap-4 mb-6">
        <div>
          <label className="block text-sm font-medium mb-1">Rango de fechas</label>
          <select className="w-full border rounded-lg p-2">
            <option>Últimos 7 días</option>
            <option>Últimos 30 días</option>
            <option>Últimos 6 meses</option>
            <option>Último año</option>
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium mb-1">Cliente</label>
          <select className="w-full border rounded-lg p-2">
            <option>Todos</option>
            <option>Empresa ABC</option>
            <option>Corporación XYZ</option>
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium mb-1">Tipo de documento</label>
          <select className="w-full border rounded-lg p-2">
            <option>Todos</option>
            <option>Contratos</option>
            <option>Propuestas</option>
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium mb-1">KPIs</label>
          <select className="w-full border rounded-lg p-2">
            <option>Tasa de éxito</option>
            <option>Volumen de documentos</option>
            <option>Sentimiento</option>
          </select>
        </div>
      </div>

      {/* Gráficas */}
      <div className="grid lg:grid-cols-2 gap-6">
        <Card title="Tendencia de Éxito en Propuestas">
          <Line data={successData} />
        </Card>

        <Card title="Distribución por Tipo de Documento">
          <Pie data={typeData} />
        </Card>

        <Card title="Análisis de Sentimiento en Correos">
          <Bar data={sentimentData} />
        </Card>

        <Card title="Tasa de Éxito por Cliente">
          <Radar data={clientSuccessData} />
        </Card>
      </div>
    </Layout>
  );
}
