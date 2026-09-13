import clsx from "clsx";
import type React from "react";
import { useEffect, useLayoutEffect, useRef, useState } from "react";
import "./dropdown.css";

interface Props {
	children: React.ReactNode;
	placement?: "left" | "right";
	onClose: () => void;
}

export const Dropdown: React.FC<Props> = ({ children, placement = "right", onClose }) => {
	const ref = useRef<HTMLDivElement>(null);
	const [style, setStyle] = useState<React.CSSProperties>();

	useEffect(() => {
		const handlePointerDown = (event: PointerEvent) => {
			const dropdown = ref.current;

			if (!dropdown) {
				return;
			}

			if (dropdown.contains(event.target as Node)) {
				return;
			}

			onClose();
		};

		const handleKeyDown = (event: KeyboardEvent) => {
			if (event.key === "Escape") {
				onClose();
			}
		};

		document.addEventListener("pointerdown", handlePointerDown);
		document.addEventListener("keydown", handleKeyDown);

		return () => {
			document.removeEventListener("pointerdown", handlePointerDown);
			document.removeEventListener("keydown", handleKeyDown);
		};
	}, [onClose]);

	useLayoutEffect(() => {
		const dropdown = ref.current;

		if (!dropdown) {
			return;
		}

		const trigger = dropdown.parentElement?.querySelector(".action-button-trigger");

		if (!(trigger instanceof HTMLElement)) {
			return;
		}

		const rect = trigger.getBoundingClientRect();
		const dropdownRect = dropdown.getBoundingClientRect();

		const gap = 6;

		let top = rect.bottom + gap;
		let left = placement === "right" ? rect.right - dropdownRect.width : rect.left;

		if (top + dropdownRect.height > window.innerHeight) {
			top = rect.top - dropdownRect.height - gap;
		}

		if (left + dropdownRect.width > window.innerWidth - gap) {
			left = window.innerWidth - dropdownRect.width - gap;
		}

		if (left < gap) {
			left = gap;
		}

		setStyle({
			top,
			left,
		});
	}, [placement]);

	return (
		<div
			ref={ref}
			className={clsx("dropdown", `dropdown - ${placement} `)}
			role="menu"
			style={style}
		>
			{children}
		</div>
	);
};

export default Dropdown;
