import { ButtonHTMLAttributes, FC } from "react";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: "primary" | "secondary";
}

const Button: FC<ButtonProps> = ({ variant = "primary", children, ...props }) => {
  const base = "px-6 py-3 rounded-lg font-semibold transition";
  const variants = {
    primary: "bg-blue-600 text-white hover:bg-indigo-700",
    secondary: "bg-gray-200 text-gray-700 hover:bg-gray-300",
  };

  return (
    <button className={`${base} ${variants[variant]}`} {...props}>
      {children}
    </button>
  );
};

export default Button;
