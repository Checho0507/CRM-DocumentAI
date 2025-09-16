// components/Layout/Layout.tsx
import { ReactNode } from "react";
import Sidebar from "./Sidebar";
import Header from "./Header";
import Footer from "./Footer";

interface LayoutProps {
  children: ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  return (
    <div className="ml-20 min-h-screen bg-red-50 ">
      <Sidebar />
      <div className="ml-64 p-6 flex flex-col min-h-screen">
        <Header />
        <main className="flex-1 flex flex-col gap-6">{children}</main>
        <Footer />
      </div>
    </div>
  );
}
