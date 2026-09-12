import clsx from "clsx";
import { LoaderCircle } from "lucide-react";
import type { ButtonHTMLAttributes, ReactNode } from "react";

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
	variant?: "primary" | "secondary" | "ghost" | "danger" | "back";
	icon?: ReactNode;
	loading?: boolean;
};

export function Button({
	variant = "secondary",
	icon,
	loading = false,
	disabled,
	children,
	className,
	...props
}: ButtonProps) {
	return (
		<button
			{...props}
			disabled={disabled || loading}
			className={clsx("button", `button-${variant}`, className)}
		>
			{loading ? <LoaderCircle size={14} className="spinner" /> : icon}
			{children}
		</button>
	);
}

type ActionButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
	children?: React.ReactNode;
	title: string;
};

export function ActionButton({ onClick, children, title }: ActionButtonProps) {
	return (
		<button
			type="button"
			className="action-button"
			aria-label={title}
			title={title}
			onClick={onClick}
		>
			{children}
		</button>
	);
}
