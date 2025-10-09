type ClientCardProps = {
  initials: string;
  name: string;
  since: string;
  stats: { value: string; label: string }[];
};

export default function ClientCard({ initials, name, since, stats }: ClientCardProps) {
  return (
    <div className="bg-white rounded-xl p-6 shadow hover:shadow-lg transition">
      {/* Header */}
      <div className="flex items-center mb-4">
        <div className="w-12 h-12 rounded-full bg-blue-600 text-white flex items-center justify-center font-semibold text-lg mr-3">
          {initials}
        </div>
        <div>
          <h3 className="font-semibold text-gray-800">{name}</h3>
          <p className="text-sm text-gray-500">Desde: {since}</p>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 mb-4">
        {stats.map((s, i) => (
          <div key={i} className="text-center">
            <div className="text-lg font-bold text-gray-800">{s.value}</div>
            <div className="text-xs text-gray-500">{s.label}</div>
          </div>
        ))}
      </div>

      {/* Actions */}
      <div className="flex justify-between">
        <button className="px-4 py-2 text-sm bg-gray-100 text-gray-800 rounded-lg hover:bg-gray-200">
          Ver detalles
        </button>
        <button className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700">
          Contactar
        </button>
      </div>
    </div>
  );
}
