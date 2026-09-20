import { X } from "lucide-react";
import { useEffect, useId, useRef } from "react";

type Props = {
	open: boolean;
	title: string;
	children: React.ReactNode;
	footer?: React.ReactNode;
	onClose: () => void;
	maxWidth?: "sm" | "md" | "lg" | "wide";
};

export function Dialog({ open, title, children, footer, onClose, maxWidth = "md" }: Props) {
	const titleId = useId();
	const dialogRef = useRef<HTMLDivElement>(null);

	useEffect(() => {
		if (!open) return;

		const previous = document.body.style.overflow;
		document.body.style.overflow = "hidden";

		const onKeyDown = (event: KeyboardEvent) => {
			if (event.key === "Escape") onClose();
		};

		window.addEventListener("keydown", onKeyDown);
		dialogRef.current?.focus();

		return () => {
			document.body.style.overflow = previous;
			window.removeEventListener("keydown", onKeyDown);
		};
	}, [open, onClose]);

	if (!open) return null;

	// A wizard needs room the three form sizes do not have: 80vw x 80vh on a desktop. On a phone
	// "80% of the viewport" is just a smaller phone, so it takes the whole screen and reads as a
	// page instead of a floating card.
	const widths = {
		sm: "max-w-[420px]",
		md: "max-w-[520px]",
		lg: "max-w-[720px]",
		wide: "max-w-[80vw] h-[80vh] max-sm:h-full max-sm:max-h-full max-sm:max-w-full max-sm:rounded-none",
	};

	const isWide = maxWidth === "wide";

	return (
		<div
			className={`fixed inset-0 z-1000 flex items-center justify-center bg-black/45 p-4 ${
				isWide ? "max-sm:p-0" : ""
			}`}
			role="presentation"
		>
			<div
				ref={dialogRef}
				tabIndex={-1}
				role="dialog"
				aria-modal="true"
				aria-labelledby={titleId}
				className={`flex max-h-[calc(100vh-40px)] w-full ${widths[maxWidth]} flex-col overflow-hidden rounded-lg border border-(--color-border) bg-(--color-surface) text-(--color-text) shadow-[0_20px_50px_rgba(0,0,0,0.20)]`}
			>
				<header className="flex min-h-14 items-center justify-between gap-4 border-b border-(--color-border) px-4 sm:px-5">
					<h2 id={titleId} className="text-base font-semibold">
						{title}
					</h2>
					<button
						type="button"
						aria-label="Close dialog"
						onClick={onClose}
						className="flex h-8 w-8 items-center justify-center rounded-md text-(--color-text-muted) hover:bg-(--color-surface-hover) hover:text-(--color-text)"
					>
						<X size={17} className="cursor-pointer" />
					</button>
				</header>

				<div className="min-h-0 overflow-y-auto p-4 sm:p-5">{children}</div>

				{footer && (
					<footer className="flex flex-col-reverse gap-2 border-t border-(--color-border) bg-(--color-surface-subtle) p-3 sm:flex-row sm:items-center sm:justify-end sm:p-4">
						{footer}
					</footer>
				)}
			</div>
		</div>
	);
}
