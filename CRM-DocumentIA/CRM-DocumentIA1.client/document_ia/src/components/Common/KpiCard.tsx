import { IconType } from "react-icons";
import {
  FaFileContract,
  FaCheckCircle,
  FaEnvelope,
  FaChartPie,
  FaUsers,
  FaChartLine,
  FaFileAlt,
  FaTimesCircle,
} from "react-icons/fa";

type KpiCardProps = {
  title: string;
  value: string;
  trend: string;
  trendUp?: boolean;
  icon:
    | "FaFileContract"
    | "FaCheckCircle"
    | "FaEnvelope"
    | "FaChartPie"
    | "FaUsers"
    | "FaChartLine"
    | "FaFileAlt"
    | "FaTimesCircle";
  color: string;
};

const icons: Record<KpiCardProps["icon"], IconType> = {
  FaFileContract,
  FaCheckCircle,
  FaEnvelope,
  FaChartPie,
  FaUsers,
  FaChartLine,
  FaFileAlt,
  FaTimesCircle,
};

export default function KpiCard({
  title,
  value,
  trend,
  trendUp = true,
  icon,
  color,
}: KpiCardProps) {
  const Icon = icons[icon];

  return (
    <div className="bg-white shadow rounded-xl p-6 flex flex-col">
      <div
        className={`w-12 h-12 ${color} text-white flex items-center justify-center rounded-lg mb-4`}
      >
        <Icon size={24} />
      </div>
      <h3 className="text-sm text-gray-500">{title}</h3>
      <p className="text-2xl font-bold text-gray-800">{value}</p>
      <p
        className={`text-sm mt-2 ${
          trendUp ? "text-green-600" : "text-red-600"
        }`}
      >
        {trend}
      </p>
    </div>
  );
}
