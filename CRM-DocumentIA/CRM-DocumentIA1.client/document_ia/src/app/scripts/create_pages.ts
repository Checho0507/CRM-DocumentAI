import fs from "fs-extra";
import path from "path";

// Template de la página
const pageTemplate = (pageName: string) => `'use client';

export default function ${pageName.charAt(0).toUpperCase() + pageName.slice(1)}() {
  return (
    <div>
      <h1 className="text-2xl font-bold">
        ${pageName.charAt(0).toUpperCase() + pageName.slice(1)}
      </h1>
      <p>Contenido de la página ${pageName}</p>
    </div>
  );
}
`;

// Lista de páginas
const pages = [
  "dashboard",
  "documents",
  "insights",
  "analytics",
  "clients",
  "settings",
  "login",
  "register",
];

pages.forEach((page) => {
  const pagePath = path.join(process.cwd(), "src", "app", page, "page.tsx");
  fs.ensureDirSync(path.dirname(pagePath));
  fs.writeFileSync(pagePath, pageTemplate(page));
  console.log(`✅ Created: ${pagePath}`);
});

console.log("🎉 All pages created successfully!");
