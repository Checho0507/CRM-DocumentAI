import { FaFilePdf, FaFileWord, FaFileExcel, FaTrashAlt, FaEye } from "react-icons/fa";

type DocumentCardProps = {
  name: string;
  type: "PDF" | "Word" | "Excel";
  size: string;
  date: string;
  onRemove: () => void;
};

export default function DocumentCard({ name, type, size, date, onRemove }: DocumentCardProps) {
  const icon =
    type === "PDF" ? (
      <FaFilePdf className="text-red-500" />
    ) : type === "Word" ? (
      <FaFileWord className="text-blue-500" />
    ) : (
      <FaFileExcel className="text-green-500" />
    );

  return (
    <div className="bg-white rounded-xl shadow p-6 flex flex-col justify-between hover:shadow-lg transition">
      <div className="flex items-center gap-3 mb-4">
        <div className="text-3xl">{icon}</div>
        <div>
          <p className="font-semibold text-gray-800 truncate">{name}</p>
          <p className="text-sm text-gray-500">{type} • {size}</p>
          <p className="text-xs text-gray-400">Subido el {date}</p>
        </div>
      </div>

      <div className="flex justify-between mt-2">
        <button className="flex items-center gap-2 text-sm px-3 py-1.5 bg-gray-100 text-gray-800 rounded-md hover:bg-gray-200">
          <FaEye /> Ver documento
        </button>
        <button
          className="flex items-center gap-2 text-sm px-3 py-1.5 bg-red-100 text-red-600 rounded-md hover:bg-red-200"
          onClick={onRemove}
        >
          <FaTrashAlt /> Eliminar
        </button>
      </div>
    </div>
  );
}
