"use client";

import Layout from "@/components/Layout/Layout";
import Card from "@/components/UI/Card";
import KpiCard from "@/components/Common/KpiCard";
import DocumentItem from "@/components/Common/DocumentItem";
import { Bar, Doughnut } from "react-chartjs-2";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
} from "chart.js";
import { useSession } from 'next-auth/react';

// Registrar módulos
ChartJS.register(CategoryScale, LinearScale, BarElement, ArcElement, Title, Tooltip, Legend);

export default function DashboardPage() {
  const { data: session, status } = useSession();
  // Datos de ejemplo
  const documentsByMonth = {
    labels: ["Abr", "May", "Jun", "Jul", "Ago", "Sep"],
    datasets: [
      {
        label: "Documentos procesados",
        data: [180, 210, 240, 280, 320, 380],
        backgroundColor: "#4361ee",
      },
    ],
  };

  const documentStatus = {
    labels: ["Aprobados", "Pendientes", "En revisión", "Rechazados"],
    datasets: [
      {
        data: [45, 25, 20, 10],
        backgroundColor: ["#4cc9f0", "#f8961e", "#4361ee", "#f72585"],
      },
    ],
  };


  if (status === 'loading') {
    return <p className="p-6">Cargando sesión...</p>;
  }

  if (!session) {
    return <p className="p-6">No estás autenticado.</p>;
  }

  return (
    <Layout>
      {/* Título y filtro */}
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Dashboard Principal</h1>
        <div className="flex items-center gap-2 bg-white shadow px-4 py-2 rounded-full">
          <i className="fas fa-calendar text-gray-500" />
          <span>Septiembre 2023</span>
          <i className="fas fa-chevron-down text-gray-500" />
        </div>
      </div>

      {/* KPI Cards */}
      <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <KpiCard
          title="Contratos Analizados"
          value="1,248"
          trend="+12.5% vs mes anterior"
          trendUp
          icon="FaFileContract"
          color="bg-blue-600"
        />
        <KpiCard
          title="Propuestas Aprobadas"
          value="342"
          trend="+8.3% vs mes anterior"
          trendUp
          icon="FaCheckCircle"
          color="bg-cyan-400"
        />
        <KpiCard
          title="Correos Relevantes"
          value="2,156"
          trend="+5.7% vs mes anterior"
          trendUp
          icon="FaEnvelope"
          color="bg-sky-500"
        />
        <KpiCard
          title="Tasa de Éxito"
          value="78.2%"
          trend="+3.2% vs mes anterior"
          trendUp
          icon="FaChartPie"
          color="bg-amber-500"
        />
      </div>

      {/* Charts */}
      <div className="grid lg:grid-cols-2 gap-6 mb-8">
        <Card title="Documentos Procesados por Mes">
          <Bar data={documentsByMonth} />
        </Card>
        <Card title="Estado de Documentos">
          <Doughnut data={documentStatus} />
        </Card>
      </div>

      {/* Alertas */}
      <Card title="Alertas y Notificaciones">
        <div className="space-y-4">
          <div className="flex items-start gap-4 p-4 bg-yellow-50 border-l-4 border-yellow-500 rounded">
            <i className="fas fa-exclamation-triangle text-yellow-500 text-lg" />
            <div className="flex-1">
              <p className="font-semibold">Contrato próximo a vencer</p>
              <p className="text-sm text-gray-600">
                El contrato con Empresa XYZ vence en 7 días
              </p>
            </div>
            <span className="text-xs text-gray-500">Hace 2h</span>
          </div>
          <div className="flex items-start gap-4 p-4 bg-blue-50 border-l-4 border-blue-500 rounded">
            <i className="fas fa-info-circle text-blue-500 text-lg" />
            <div className="flex-1">
              <p className="font-semibold">Nuevo insight generado</p>
              <p className="text-sm text-gray-600">
                Cliente ABC es candidato para upselling
              </p>
            </div>
            <span className="text-xs text-gray-500">Ayer</span>
          </div>
          <div className="flex items-start gap-4 p-4 bg-green-50 border-l-4 border-green-500 rounded">
            <i className="fas fa-check-circle text-green-500 text-lg" />
            <div className="flex-1">
              <p className="font-semibold">Propuesta aprobada</p>
              <p className="text-sm text-gray-600">
                La propuesta para Proyecto Alpha fue aprobada
              </p>
            </div>
            <span className="text-xs text-gray-500">12 Sep</span>
          </div>
        </div>
      </Card>

      {/* Documentos recientes */}
      <Card title="Documentos Recientes">
        <div className="space-y-4">
          <DocumentItem
            name="Contrato de Servicios.pdf"
            client="Empresa ABC"
            date="15 Sep 2023"
            size="2.4 MB"
            icon="FaFilePdf"
          />
          <DocumentItem
            name="Propuesta Comercial.docx"
            client="Corporación XYZ"
            date="14 Sep 2023"
            size="1.8 MB"
            icon="FaFileWord"
          />
          <DocumentItem
            name="Negociación Cliente Final.eml"
            client="Tech Solutions"
            date="13 Sep 2023"
            size="0.8 MB"
            icon="FaEnvelope"
          />
        </div>
      </Card>
    </Layout>
  );
}
