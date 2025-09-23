import Layout from "@/components/Layout/Layout";
import Button from "@/components/UI/Button";
import Card from "@/components/UI/Card";
import { FaFileContract, FaChartPie, FaRobot } from "react-icons/fa";
import "./globals.css";

export default function Home() {
  return (
    <Layout>
      {/* Hero */}
      <section className="bg-gradient-to-r from-blue-600 to-indigo-700 text-white rounded-xl p-12 text-center mb-8">
        <h1 className="text-4xl font-bold mb-4">CRM Inteligente para tu Empresa</h1>
        <p className="text-lg mb-6 max-w-2xl mx-auto">
          Gestiona tus documentos, clientes y oportunidades comerciales con nuestra
          plataforma impulsada por IA
        </p>
        <Button>Comenzar ahora</Button>
      </section>

      {/* Features */}
      <section className="grid md:grid-cols-3 gap-6 mb-8">
        <Card>
          <div className="w-16 h-16 rounded-full bg-blue-600 flex items-center justify-center text-white text-2xl mx-auto mb-4">
            <FaFileContract />
          </div>
          <h3 className="text-xl font-semibold mb-2">Gestión de Documentos</h3>
          <p className="text-gray-600">
            Analiza y gestiona contratos, propuestas y correos con nuestra tecnología de IA
          </p>
        </Card>

        <Card>
          <div className="w-16 h-16 rounded-full bg-blue-600 flex items-center justify-center text-white text-2xl mx-auto mb-4">
            <FaChartPie />
          </div>
          <h3 className="text-xl font-semibold m-2">Analítica Avanzada</h3>
          <p className="text-gray-600">
            Obtén insights valiosos sobre tus clientes y el rendimiento de tus propuestas
          </p>
        </Card>

        <Card>
          <div className="w-16 h-16 rounded-full bg-blue-600 flex items-center justify-center text-white text-2xl mx-auto mb-4">
            <FaRobot />
          </div>
          <h3 className="text-xl font-semibold mb-2">Recomendaciones Inteligentes</h3>
          <p className="text-gray-600">
            Sugerencias automatizadas para mejorar tus relaciones comerciales
          </p>
        </Card>
      </section>

      {/* Info Card */}
      <Card title="¿Cómo funciona?">
        <p className="text-gray-700 mb-2">
          Nuestro sistema utiliza procesamiento de lenguaje natural y modelos de machine learning
          para analizar tus documentos y extraer información valiosa que te ayuda a tomar mejores
          decisiones comerciales.
        </p>
        <p className="text-gray-700">
          Comienza subiendo tus documentos y en minutos obtendrás análisis detallados, alertas
          importantes y recomendaciones accionables.
        </p>
      </Card>
    </Layout>
  );
}
