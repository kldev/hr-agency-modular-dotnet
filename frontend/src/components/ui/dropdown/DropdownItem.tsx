import type React from "react";

interface Props {
	children: React.ReactNode;
	onClick?: () => void;
}

const DropdownItem: React.FC<Props> = ({ children, onClick }) => {
	return (
		<button onClick={onClick} type="button" className="dropdown-item" role="menuitem">
			{children}
		</button>
	);
};

export default DropdownItem;
