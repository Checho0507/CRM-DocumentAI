import { FC } from "react";
import Link from "next/link";

const Footer: FC = () => {
  return (
    <footer className="bg-white shadow mt-8 rounded-xl p-6 text-center text-gray-600">
      <div className="flex flex-col md:flex-row items-center justify-between gap-4">
        <p className="text-sm">
          © {new Date().getFullYear()} CRM Inteligente. Todos los derechos reservados.
        </p>

        <div className="flex gap-6 text-sm">
          <Link href="/privacy" className="hover:text-blue-600 transition">
            Privacidad
          </Link>
          <Link href="/terms" className="hover:text-blue-600 transition">
            Términos
          </Link>
          <Link href="/contact" className="hover:text-blue-600 transition">
            Contacto
          </Link>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
