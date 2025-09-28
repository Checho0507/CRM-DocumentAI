import { FaFilePdf, FaFileWord, FaEnvelope } from "react-icons/fa";
import { IconType } from "react-icons";

type DocumentItemProps = {
  name: string;
  client: string;
  date: string;
  size: string;
  icon: "FaFilePdf" | "FaFileWord" | "FaEnvelope";
};

const icons: Record<DocumentItemProps["icon"], IconType> = {
  FaFilePdf,
  FaFileWord,
  FaEnvelope,
};

export default function DocumentItem({
  name,
  client,
  date,
  size,
  icon,
}: DocumentItemProps) {
  const Icon = icons[icon];
  return (
    <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg hover:bg-gray-100">
      <div className="flex items-center gap-4">
        <Icon className="text-blue-600 text-2xl" />
        <div>
          <p className="font-medium text-gray-800">{name}</p>
          <p className="text-sm text-gray-500">
            {client} • {date}
          </p>
        </div>
      </div>
      <span className="text-sm text-gray-500">{size}</span>
    </div>
  );
}
