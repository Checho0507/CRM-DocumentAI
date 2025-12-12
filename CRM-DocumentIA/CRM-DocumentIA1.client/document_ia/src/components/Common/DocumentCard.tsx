// components/Common/DocumentCard.tsx
import { FaFilePdf, FaFileWord, FaFileExcel, FaTrashAlt, FaEye, FaDownload, FaShareAlt } from "react-icons/fa";

type DocumentCardProps = {
  name: string;
  type: "PDF" | "Word" | "Excel";
  size: string;
  date: string;
  onRemove: () => void;
  onView?: () => void;
  onDownload?: () => void;
  onShare?: () => void; // Nueva prop
  isSelected?: boolean;
  onSelect?: () => void;
};

export default function DocumentCard({ 
  name, 
  type, 
  size, 
  date, 
  onRemove, 
  onView, 
  onDownload,
  onShare,
  isSelected = false,
  onSelect
}: DocumentCardProps) {
  
  const icon =
    type === "PDF" ? (
      <FaFilePdf className="text-red-500" />
    ) : type === "Word" ? (
      <FaFileWord className="text-blue-500" />
    ) : (
      <FaFileExcel className="text-green-500" />
    );

  return (
    <div className={`
      bg-white rounded-xl shadow p-6 flex flex-col justify-between 
      hover:shadow-lg transition-all duration-200 border-2
      ${isSelected ? 'border-blue-500 bg-blue-50' : 'border-transparent'}
      hover:border-gray-200 cursor-pointer
    `}
    onClick={onSelect}
    >
      <div className="flex items-center gap-3 mb-4">
        <div className="text-3xl">{icon}</div>
        <div className="flex-1 min-w-0">
          <p className="font-semibold text-gray-800 truncate">
            {name}
          </p>
          <p className="text-sm text-gray-500 mt-1">
            {type} • {size}
          </p>
          <p className="text-xs text-gray-400 mt-1">
            Subido el {date}
          </p>
        </div>
      </div>

      <div className="flex justify-between items-center mt-4 pt-3 border-t border-gray-100">
        <div className="flex gap-2">
          {onView && (
            <button
              className="flex items-center gap-2 text-sm px-3 py-1.5 bg-blue-100 text-blue-600 rounded-md hover:bg-blue-200 transition"
              onClick={(e) => {
                e.stopPropagation();
                onView();
              }}
              title="Ver documento"
            >
              <FaEye /> Ver
            </button>
          )}
          
          {onShare && (
            <button
              className="flex items-center gap-2 text-sm px-3 py-1.5 bg-purple-100 text-purple-600 rounded-md hover:bg-purple-200 transition"
              onClick={(e) => {
                e.stopPropagation();
                onShare();
              }}
              title="Compartir documento"
            >
              <FaShareAlt /> Compartir
            </button>
          )}
        </div>
        
        <div className="flex gap-2">
          {onDownload && (
            <button
              className="flex items-center gap-2 text-sm px-3 py-1.5 bg-gray-100 text-gray-600 rounded-md hover:bg-gray-200 transition"
              onClick={(e) => {
                e.stopPropagation();
                onDownload();
              }}
              title="Descargar"
            >
              <FaDownload />
            </button>
          )}
          
          <button
            className="flex items-center gap-2 text-sm px-3 py-1.5 bg-red-100 text-red-600 rounded-md hover:bg-red-200 transition"
            onClick={(e) => {
              e.stopPropagation();
              onRemove();
            }}
            title="Eliminar documento"
          >
            <FaTrashAlt />
          </button>
        </div>
      </div>
    </div>
  );
}