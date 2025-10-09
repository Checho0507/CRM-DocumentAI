import { JSX } from "react";
import {
  FaExclamationTriangle,
  FaArrowUp,
  FaClock,
  FaHandshake,
  FaFileContract,
  FaComments,
} from "react-icons/fa";

type InsightCardProps = {
  type: "risk" | "opportunity" | "warning";
  icon: string;
  title: string;
  desc: string;
  content: string;
  action: string;
};

const iconMap: Record<string, JSX.Element> = {
  FaExclamationTriangle: <FaExclamationTriangle />,
  FaArrowUp: <FaArrowUp />,
  FaClock: <FaClock />,
  FaHandshake: <FaHandshake />,
  FaFileContract: <FaFileContract />,
  FaComments: <FaComments />,
};

const typeClasses: Record<string, string> = {
  risk: "bg-red-100 text-red-600",
  opportunity: "bg-green-100 text-green-600",
  warning: "bg-yellow-100 text-yellow-600",
};

export default function InsightCard({
  type,
  icon,
  title,
  desc,
  content,
  action,
}: InsightCardProps) {
  return (
    <div className="bg-white rounded-xl p-6 shadow hover:shadow-lg transition">
      {/* Header */}
      <div className="flex items-center mb-4">
        <div
          className={`w-10 h-10 flex items-center justify-center rounded-lg mr-3 text-lg ${typeClasses[type]}`}
        >
          {iconMap[icon]}
        </div>
        <div>
          <h3 className="font-semibold text-gray-800">{title}</h3>
          <p className="text-sm text-gray-500">{desc}</p>
        </div>
      </div>

      {/* Body */}
      <p className="text-sm text-gray-600 mb-4">{content}</p>

      {/* Action */}
      <div className="flex justify-end">
        <button className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700">
          {action}
        </button>
      </div>
    </div>
  );
}
