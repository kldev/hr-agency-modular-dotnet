import clsx from "clsx";
import type React from "react";
import { useLayoutEffect, useRef, useState } from "react";
import "./dropdown.css";

interface Props {
	children: React.ReactNode;
	placement?: "left" | "right";
}

export const Dropdown: React.FC<Props> = ({ children, placement = "right" }) => {
	const ref = useRef<HTMLDivElement>(null);
	const [style, setStyle] = useState<React.CSSProperties>();

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

		// Jeżeli nie ma miejsca pod przyciskiem,
		// otwórz menu nad nim.
		if (top + dropdownRect.height > window.innerHeight) {
			top = rect.top - dropdownRect.height - gap;
		}

		// Nie wychodź poza prawą krawędź viewportu.
		if (left + dropdownRect.width > window.innerWidth - gap) {
			left = window.innerWidth - dropdownRect.width - gap;
		}

		// Nie wychodź poza lewą krawędź viewportu.
		if (left < gap) {
			left = gap;
		}

		setStyle({
			top,
			left,
		});
	}, [placement]);

	return (
		<div ref={ref} className={clsx("dropdown", `dropdown-${placement}`)} role="menu" style={style}>
			{children}
		</div>
	);
};

export default Dropdown;
