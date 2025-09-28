"use client";

import { FC } from "react";
import {
  FaBrain,
  FaHome,
  FaTachometerAlt,
  FaFileAlt,
  FaLightbulb,
  FaChartLine,
  FaUsers,
  FaCog,
  FaSignOutAlt,
} from "react-icons/fa";
import Link from "next/link";
import { usePathname } from "next/navigation";

const menuItems = [
  { icon: <FaHome />, label: "Inicio", href: "/" },
  { icon: <FaTachometerAlt />, label: "Dashboard", href: "/dashboard" },
  { icon: <FaFileAlt />, label: "Documentos", href: "/documents" },
  { icon: <FaLightbulb />, label: "Insights", href: "/insights" },
  { icon: <FaChartLine />, label: "Analítica", href: "/analytics" },
  { icon: <FaUsers />, label: "Clientes", href: "/clients" },
  { icon: <FaCog />, label: "Configuración", href: "/settings" },
  { icon: <FaSignOutAlt />, label: "Cerrar Sesión", href: "/login" },
];

const Sidebar: FC = () => {
  const pathname = usePathname();

  return (
    <aside className="w-64 bg-gradient-to-b from-blue-600 to-indigo-900 text-white h-screen fixed left-0 top-0 p-5">
      <div className="mb-8 border-b border-white/20 pb-4">
        <h2 className="flex items-center gap-2 text-xl font-bold">
          <FaBrain className="text-2xl" /> <span>CRM Inteligente</span>
        </h2>
      </div>
      <ul className="space-y-2">
        {menuItems.map(({ icon, label, href }) => (
          <li key={label}>
            <Link
              href={href}
              className={`flex items-center gap-3 px-4 py-2 rounded-md transition ${pathname === href
                  ? "bg-white/20 border-l-4 border-white"
                  : "hover:bg-white/10"
                }`}
            >
              {icon}
              <span>{label}</span>
            </Link>
          </li>
        ))}
      </ul>
    </aside>
  );
};

export default Sidebar;
