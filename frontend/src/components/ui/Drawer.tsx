import clsx from "clsx";
import { X } from "lucide-react";
import { type ReactNode, useEffect } from "react";

interface DrawerProps {
	open: boolean;
	title: string;
	children: ReactNode;
	footer: ReactNode;
	onClose: () => void;
}

export function Drawer({ open, title, children, footer, onClose }: DrawerProps) {
	useEffect(() => {
		if (!open) return;

		const handleKeyDown = (event: KeyboardEvent) => {
			if (event.key === "Escape") onClose();
		};

		document.addEventListener("keydown", handleKeyDown);
		document.body.style.overflow = "hidden";

		return () => {
			document.removeEventListener("keydown", handleKeyDown);
			document.body.style.overflow = "";
		};
	}, [open, onClose]);

	if (!open) return null;

	return (
		<>
			{/** biome-ignore lint/a11y/noStaticElementInteractions: false */}
			<div className="drawer-overlay" onMouseDown={onClose} />
			<aside
				className={clsx("drawer")}
				role="dialog"
				aria-modal="true"
				aria-labelledby="drawer-title"
				onMouseDown={(event) => event.stopPropagation()}
			>
				<header className="drawer-header">
					<h2 id="drawer-title" className="drawer-title">
						{title}
					</h2>
					<button
						type="button"
						className="action-button"
						aria-label="Close"
						title="Close"
						onClick={onClose}
					>
						<X size={17} className="cursor-pointer" />
					</button>
				</header>

				<div className="drawer-content">{children}</div>
				<footer className="drawer-footer">{footer}</footer>
			</aside>
		</>
	);
}
