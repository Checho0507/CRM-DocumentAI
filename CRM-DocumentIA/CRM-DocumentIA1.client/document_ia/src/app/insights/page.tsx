"use client";

import Layout from "@/components/Layout/Layout";
import {ChatPreview} from "@/components/Chat/ChatPreview";
import { API_ROUTES } from "@/lib/apiRoutes";
import axios from "axios";
import { useSession } from "next-auth/react";
import { useEffect, useState } from "react";
import { FaPaperPlane, FaTimes, FaChevronDown, FaQuestionCircle, FaCopy, FaThumbsUp, FaThumbsDown, FaTrash } from "react-icons/fa";

interface ChatObj
{
  id: number,
  title: string,
  createdAt: string,
  userId: number
}

interface ChatBotBody
{
  id:number,
  from: string,
  text: string
}

interface InsightBody
{
  id: number,
  userId: number,
  name: string,
  answer: string,
  question: string,
  chatId: number
}

interface HistoChat
{
  insights: InsightBody[]
}

export default function InsightsPage() {
  const [query, setQuery] = useState("");
  const [loading, setLoading] = useState(true);
  const { data: session, status } = useSession();
  const [sources, setSources] = useState<ChatBotBody[]>([]);
  const [response, setResponse] = useState<string | null>(null);
  const [chatResponse, setChatResponse] = useState<ChatObj[]>();
  const [chatIdSelected, setChatIdSelected] = useState<number|null>();

  const fetchGetChats = async () => {
    if (!session?.user?.id) {
      return;
    }

    try {
      setLoading(true);
      const response = await axios.get<ChatObj[]>(
        `${API_ROUTES.ASK_CHATS}/${session.user.id}`,
      );

      console.log("📄 Chats cargados desde BD:", response.data);
      if(response.data != undefined)
      {
        setChatResponse(response.data);
      }

    } catch (error: unknown) {
      console.error("❌ Error al obtener documentos:", error);
      if (axios.isAxiosError(error)) {
        console.error("📊 Respuesta del error:", error.response?.data);
      }
    } finally {
      setLoading(false);
    }
  };

  const fetchGetHistoInsight = async (chatId:number) => {
    if (!session?.user?.id) {
      return;
    }

    try {
      setLoading(true);
      const response = await axios.get<HistoChat>(`${API_ROUTES.HISTO_CHATS}/${chatId}`);

      if(response.data != undefined)
      {
        setChatIdSelected(chatId);
        setSources(mapInsightsToChat(response.data));
      }

    } catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        console.error("📊 Respuesta del error:", error.response?.data);
      }
    } finally {
      setLoading(false);
    }
  };

  const fetchDeleteChat = async (chatId:number) => {
    if (!session?.user?.id) {
      return;
    }

    try {
      setLoading(true);
      await axios.delete<string>(`${API_ROUTES.DELETE_CHAT}/${chatId}`,);
      void fetchGetChats();

    } catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        console.error("📊 Respuesta del error:", error.response?.data);
      }
    } finally {
      setLoading(false);
    }
  };

  const mapInsightsToChat =(chat: HistoChat) => {
    const messages: { from: "user" | "ai"; text: string, id:number }[] = [];

    console.info(chat);
    chat.insights.forEach(insight => {
        messages.push({
          id: insight.id,
          from: "user",
          text: insight.question
        });

        messages.push({
          id: insight.id,
          from: "ai",
          text: insight.answer
        });
    });

    return messages;
  }
  
  // 🔹 Cargar documentos cuando la sesión esté disponible
  useEffect(() => {
    if (status === "authenticated" && session?.user?.id) {  
      fetchGetChats();
    }
  }, [status, session]); // ✅ Agregué las dependencias correctas


  const timeAgo = (dateString: string): string => {
    const date = new Date(dateString);
    const now = new Date();
    const diff = (now.getTime() - date.getTime()) / 1000; // diferencia en segundos

    const seconds = Math.floor(diff);
    const minutes = Math.floor(diff / 60);
    const hours = Math.floor(diff / 3600);
    const days = Math.floor(diff / 86400);
    const weeks = Math.floor(diff / 604800);
    const months = Math.floor(diff / 2592000);
    const years = Math.floor(diff / 31536000);

    if (seconds < 60) return "Hace un momento";
    if (minutes < 60) return `Hace ${minutes} minuto${minutes !== 1 ? "s" : ""}`;
    if (hours < 24) return `Hace ${hours} hora${hours !== 1 ? "s" : ""}`;
    if (days < 7) return `Hace ${days} día${days !== 1 ? "s" : ""}`;
    if (weeks < 4) return `Hace ${weeks} semana${weeks !== 1 ? "s" : ""}`;
    if (months < 12) return `Hace ${months} mes${months !== 1 ? "s" : ""}`;

    return `Hace ${years} año${years !== 1 ? "s" : ""}`;
  }

  const handleAsk = async () => {
    if (!query.trim()) return;
    console.log(query);
    try {

      const response = await axios.post(API_ROUTES.INSIGHT_ASK, {query: query,userId:session?.user?.id,chatId:chatIdSelected });

      console.log("✅ Pregunta Generada:", response.data);
      setQuery("");

      // 🔹 Recargar TODOS los documentos desde la BD después de subir
      await fetchGetHistoInsight(response.data.chatId);
      await fetchGetChats();

    } catch (error: unknown) {
      console.error("❌ Error al subir documento:", error);
      if (axios.isAxiosError(error)) {
        console.error("📊 Detalles del error:", error.response?.data);
      }
    }
  };

  const newChat = ()=>{
    setChatIdSelected(null);
    setSources([]);
  }

  return (
    <Layout>
      {/* Header */}

      {/* Title */}
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Insights - Consultas RAG</h1>
        <button className="flex items-center gap-2 bg-white px-4 py-2 rounded-full shadow cursor-pointer" onClick={()=>{void newChat()}}>
          <i className="fas fa-calendar text-gray-500" />
          <span>Nuevo Chat</span>
          <FaChevronDown className="text-gray-500" />
        </button>
      </div>

      <ChatPreview messagesHisto={sources}/>

      {/* Query Section */}
      <div className="bg-white p-6 rounded-xl shadow mb-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold">Consulta de Documentos</h2>
        </div>

        <textarea
          className="w-full border border-gray-300 rounded-xl p-3 min-h-[100px] mb-4 focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="Escribe tu pregunta sobre los documentos..."
          value={query}
          onChange={(e) => setQuery(e.target.value)}
        />

        <div className="flex justify-end gap-2">
          <button
            className="px-4 py-2 border border-blue-600 text-blue-600 rounded-md flex items-center gap-2 cursor-pointer"
            onClick={() => {
              setQuery("");
              setResponse(null);
            }}
          >
            <FaTimes /> Limpiar
          </button>
          <button
            className="px-4 py-2 bg-blue-600 text-white rounded-md flex items-center gap-2 cursor-pointer"
            onClick={()=>{void handleAsk()}}
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

          </div>
        )}
      </div>

      {/* History Section */}
      <div className="bg-white p-6 rounded-xl shadow">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold">Consultas Recientes</h2>
          
        </div>

        <div className="grid gap-3">

        { chatResponse !== undefined && chatResponse.length > 0 ? (
            chatResponse.map((chat) => (

              <div key={chat.id} className="p-4 bg-gray-100 rounded-lg cursor-pointer hover:bg-blue-50 flex" onClick={()=>{void fetchGetHistoInsight(chat.id)}}>

                <div className="p-4 w-4/5">
                  <p className="font-medium flex items-center gap-2" >
                    <FaQuestionCircle className="text-blue-600" /> {chat.title}
                  </p>

                  <p className="text-xs text-gray-500 mt-2">{timeAgo(chat.createdAt)}</p>
                </div>
                <div className="p-4 w-1/5">
                  {/* Botón eliminar */}
                  <button className=" text-red-500 hover:text-red-700 cursor-pointer gap-2 " style={{fontSize: "20px"}} onClick={()=>{void fetchDeleteChat(chat.id)}}>
                    <FaTrash />
                  </button>
                </div>
              </div>

            ))
            ): (
            <div className="col-span-full text-center py-8">
              <p className="text-gray-600">No hay chats activos aún.</p>
              <p className="text-sm text-gray-500 mt-2">
                Inicia una conversación para tener historicos
              </p>
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
}
