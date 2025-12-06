"use client";

import axios from "axios";
import Card from "@/components/UI/Card";
import { API_ROUTES } from "@/lib/apiRoutes";
import { useSession } from 'next-auth/react';
import Layout from "@/components/Layout/Layout";
import { Bar, Doughnut } from "react-chartjs-2";
import KpiCard from "@/components/Common/KpiCard";
import DocumentItem from "@/components/Common/DocumentItem";

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
import { useEffect, useState } from "react";

// Registrar módulos
ChartJS.register(CategoryScale, LinearScale, BarElement, ArcElement, Title, Tooltip, Legend);



interface SummaryObj
{
  totalUsuarios: number,
  totalChats: number,
  totalConsultas: number,
  usuariosActivosUltimos30Dias: number
}

interface DaillyObj
{
  fecha: string,
  cantidad: number
}

interface DocsStatusObj
{
  estado: string,
  cantidad: number
}

interface DocsTypesObj
{
  tipo: string,
  cantidad: number
}

export default function DashboardPage() {

  const { data: session, status } = useSession();
  const [summary, setSummary] = useState<SummaryObj>();
  const [labelsMonth, setLabelsMonth] = useState<string[]>([]);
  const [valuesMonth, setValuesMonth] = useState<number[]>([]);

  const [labelsPie, setLabelsPie] = useState<string[]>([]);
  const [valuesPie, setValuesPie] = useState<number[]>([]);

  const [labelsPieType, setLabelsPieType] = useState<string[]>([]);
  const [valuesPieType, setValuesPieType] = useState<number[]>([]);
  

  const fetchGetSummary = async () => {
    if (!session?.user?.id) {
      return;
    }

    try {
      const response = await axios.get<SummaryObj>(`${API_ROUTES.SUMMARY_CHART}`);

      if(response.data != undefined)
      {
        setSummary(response.data);
      }

    } catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        console.error("📊 Respuesta del error:", error.response?.data);
      }
    } 
  };

  const fetchGetQueryDailly = async () => {
    if (!session?.user?.id) {
      return;
    }

    try {
      const response = await axios.get<DaillyObj[]>(`${API_ROUTES.QUERY_DAILLY}`);

      if(response.data != undefined)
      {
        const {labels, valores} = convertirConsultasPorMes(response.data);

        setLabelsMonth(labels);
        setValuesMonth(valores);
        
      }

    } catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        console.error("📊 Respuesta del error:", error.response?.data);
      }
    } 
  };

  const fetchGetDocsStatus = async () => {
    if (!session?.user?.id) {
      return;
    }

    try {
      const response = await axios.get<DocsStatusObj[]>(`${API_ROUTES.DOCS_STATUS}`);

      if(response.data != undefined)
      {
        const {labels, data} = mapDocumentStatusToChart(response.data);

        setLabelsPie(labels);
        setValuesPie(data);
        
      }

    } catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        console.error("📊 Respuesta del error:", error.response?.data);
      }
    } 
  };

  const fetchGetDocsTypes = async () => {
    if (!session?.user?.id) {
      return;
    }

    try {
      const response = await axios.get<DocsTypesObj[]>(`${API_ROUTES.DOCS_TYPES}`);

      if(response.data != undefined)
      {
        const {labels, data} = mapDocumentTypesToChart(response.data);

        setLabelsPieType(labels);
        setValuesPieType(data);
        console.info(data);
        
      }

    } catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        console.error("📊 Respuesta del error:", error.response?.data);
      }
    } 
  };

  const convertirConsultasPorMes = (data:DaillyObj[]) => {
    // Nombres de meses abreviados en español
    const meses = ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"];

    // Objeto auxiliar para acumular sumas por mes
    const acumulado: { [mes: number]: number } = {};

    data.forEach(item => {
      const fecha = new Date(item.fecha);
      const mesIndex = fecha.getMonth(); // 0–11
      
      if (!acumulado[mesIndex]) {
        acumulado[mesIndex] = 0;
      }

      acumulado[mesIndex] += item.cantidad;
    });

    // Ordenar meses por número
    const mesesOrdenados = Object.keys(acumulado)
      .map(m => parseInt(m))
      .sort((a, b) => a - b);

    // Convertir índices a nombres de mes
    const labels = mesesOrdenados.map(i => meses[i]);

    // Extraer valores del acumulado en el mismo orden
    const valores = mesesOrdenados.map(i => acumulado[i]);

    return { labels, valores };
  }

  const mapDocumentStatusToChart = (items: { estado: string; cantidad: number }[]) => {
    // Mapeo de estados → etiquetas
    const labelMap: Record<string, string> = {
        completado: "Aprobados",
        pendiente: "Pendientes",
        error: "Error"
    };

    // Orden deseado en la gráfica
    const order = ["completado", "pendiente", "error"];

    const labels: string[] = [];
    const data: number[] = [];

    order.forEach((estado) => {
        const item = items.find(i => i.estado === estado);
        labels.push(labelMap[estado]);
        data.push(item ? item.cantidad : 0);  // Si no existe, 0
    });

    return { labels, data };
  };

  const mapDocumentTypesToChart = (items: { tipo: string; cantidad: number }[]) => {
    // Mapeo de estados → etiquetas
    const labelMap: Record<string, string> = {
        doc: "Doc",
        docx: "Docx",
        pdf: "Pdf",
        txt: "Txt",
    };

    // Orden deseado en la gráfica
    const order = [".doc", ".docx", ".pdf",".txt"];

    const labels: string[] = [];
    const data: number[] = [];

    order.forEach((tipo) => {
        const item = items.find(i => i.tipo === tipo);
        labels.push(labelMap[tipo.split(".")[1]]);
        data.push(item ? item.cantidad : 0);  // Si no existe, 0
    });

    return { labels, data };
  };

  
  useEffect(() => {
    if (status === "authenticated" && session?.user?.id) {  
      fetchGetSummary();
      fetchGetDocsTypes();
      fetchGetDocsStatus();
      fetchGetQueryDailly();
    }
  }, [status, session]); // ✅ Agregué las dependencias correctas

  // Datos de ejemplo
  const documentsByMonth = {
    labels: labelsMonth,
    datasets: [
      {
        label: "Documentos procesados",
        data: valuesMonth,
        backgroundColor: "#4361ee",
      },
    ],
  };

  const documentStatus = {
    labels: labelsPie,
    datasets: [
      {
        data: valuesPie,
        backgroundColor: ["#4cc9f0", "#f8961e", "#4361ee", "#f72585"],
      },
    ],
  };

  const documentTypes = {
    labels: labelsPieType,
    datasets: [
      {
        data: valuesPieType,
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
      </div>

      {/* KPI Cards */}
      {summary && (
        <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <KpiCard
            title="Total Usuario"
            value={summary.totalUsuarios.toString()}
            trend="+12.5% vs mes anterior"
            trendUp
            icon="FaFileContract"
            color="bg-blue-600"
          />
          <KpiCard
            title="Chat Totales"
            value={summary.totalChats.toString()}
            trend="+8.3% vs mes anterior"
            trendUp
            icon="FaCheckCircle"
            color="bg-cyan-400"
          />
          <KpiCard
            title="Consultas Totales"
            value={summary.totalConsultas.toString()}
            trend="+5.7% vs mes anterior"
            trendUp
            icon="FaEnvelope"
            color="bg-sky-500"
          />
          <KpiCard
            title="Usuario Activo en los últimos 30 días"
            value={summary.usuariosActivosUltimos30Dias.toString()}
            trend="+3.2% vs mes anterior"
            trendUp
            icon="FaUsers"
            color="bg-amber-500"
          />
        </div>
      )}

      {/* Charts */}
      <div className="grid lg:grid-cols-2 gap-6 mb-8">
        <Card title="Documentos Procesados por Mes">
          <Bar data={documentsByMonth} />
        </Card>
        <Card title="Estados de Documentos">
          <Doughnut data={documentStatus} />
        </Card>
        <Card title="Tipos de Documentos">
          <Doughnut data={documentTypes} />
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
