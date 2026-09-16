import clsx from "clsx";
import { X } from "lucide-react";
import type { FormHTMLAttributes, ReactNode } from "react";
import { useEffect } from "react";

type FormDrawerProps = Omit<FormHTMLAttributes<HTMLFormElement>, "children"> & {
	open: boolean;
	title: string;
	children: ReactNode;
	onClose: () => void;
};

type FormDrawerContentProps = {
	children: ReactNode;
	className?: string;
};

type FormDrawerFooterProps = {
	children: ReactNode;
	className?: string;
};

function FormDrawer({ open, title, children, onClose, className, ...formProps }: FormDrawerProps) {
	useEffect(() => {
		if (!open) {
			return;
		}

		const handleKeyDown = (event: KeyboardEvent) => {
			if (event.key === "Escape") {
				onClose();
			}
		};

		document.addEventListener("keydown", handleKeyDown);
		document.body.style.overflow = "hidden";

		return () => {
			document.removeEventListener("keydown", handleKeyDown);
			document.body.style.overflow = "";
		};
	}, [open, onClose]);

	if (!open) {
		return null;
	}

	return (
		<>
			<div className="drawer-overlay" />

			<form {...formProps} className={clsx("drawer", className)}>
				<header className="drawer-header">
					<h2 className="drawer-title">{title}</h2>

					<button
						type="button"
						className="action-button"
						aria-label="Close"
						title="Close"
						onClick={onClose}
					>
						<X size={17} />
					</button>
				</header>

				{children}
			</form>
		</>
	);
}

function Content({ children, className }: FormDrawerContentProps) {
	return <div className={clsx("drawer-content", className)}>{children}</div>;
}

function Footer({ children, className }: FormDrawerFooterProps) {
	return <footer className={clsx("drawer-footer", className)}>{children}</footer>;
}

export { FormDrawer };

FormDrawer.Content = Content;
FormDrawer.Footer = Footer;
