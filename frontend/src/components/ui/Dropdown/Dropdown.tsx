import React from "react";
import "./dropdown.css";

interface Props {
	children: React.ReactNode;
}

const Dropdown: React.FC<Props> = ({ children }) => {
	return (
		<div className="dropdown" role="menu">
			{children}
		</div>
	);
};

export default Dropdown;
