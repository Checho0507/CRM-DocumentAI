import { FC } from "react";
import { FaSearch, FaBell, FaChevronDown } from "react-icons/fa";

const Header: FC = () => {
  return (
    <header className="flex justify-between items-center bg-white p-4 rounded-xl shadow mb-6">
      {/* Search Bar */}
      <div className="flex items-center bg-gray-100 rounded-full px-4 py-2 w-72">
        <FaSearch className="text-gray-500" />
        <input
          type="text"
          placeholder="Buscar..."
          className="ml-2 bg-transparent outline-none w-full"
        />
      </div>

      {/* User Actions */}
      <div className="flex items-center gap-6">
        <div className="relative cursor-pointer">
          <FaBell className="text-gray-600 text-lg" />
          <span className="absolute -top-1 -right-1 bg-pink-600 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
            3
          </span>
        </div>
        <div className="flex items-center gap-2 cursor-pointer">
          <div className="w-10 h-10 rounded-full bg-blue-600 flex items-center justify-center text-white font-semibold">
            AM
          </div>
          <span>Ana Martínez</span>
          <FaChevronDown />
        </div>
      </div>
    </header>
  );
};

export default Header;
