import type React from "react";
import "./dropdown.css";
import clsx from "clsx";

interface Props {
	children: React.ReactNode;
	placement?: "left" | "right";
}

const Dropdown: React.FC<Props> = ({ children, placement }) => {
	return (
		<div className={clsx("dropdown", `dropdown-${placement}`)} role="menu">
			{children}
		</div>
	);
};

export default Dropdown;
