interface ChatBotBody
{
  id: number,
  from: string,
  text: string
}

interface ChatPreviewProps {
  messagesHisto: ChatBotBody[];
}

export const ChatPreview = ({ messagesHisto }: ChatPreviewProps) => {
  
  return (
    <div className="bg-white p-4 rounded-xl shadow mb-6 max-h-[950px] overflow-y-auto">
      <h2 className="text-lg font-semibold mb-3">Conversación reciente</h2>

      <div className="space-y-3">
        {messagesHisto.map((msg:ChatBotBody,idx) => (
          <div
            key={idx + msg.id}
            className={`flex ${
              msg.from === "user" ? "justify-end" : "justify-start"
            }`}
          >
            <div
              className={`px-4 py-2 rounded-xl max-w-xs text-sm ${
                msg.from === "user"
                  ? "bg-blue-600 text-white rounded-br-none"
                  : "bg-gray-200 text-gray-800 rounded-bl-none"
              }`}
            >
              {msg.text}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
