import { FC, ReactNode } from "react";

interface CardProps {
  title?: string;
  children: ReactNode;
}

const Card: FC<CardProps> = ({ title, children }) => {
  return (
    <div className="bg-white rounded-xl p-6 shadow mb-6">
      {title && <h2 className="text-lg font-semibold mb-4">{title}</h2>}
      {children}
    </div>
  );
};

export default Card;
