import type React from "react";

interface Props {
	children: React.ReactNode;
	onClick?: () => void;
	disabled?: boolean;
}

const DropdownItem: React.FC<Props> = ({ children, onClick, disabled }) => {
	return (
		<button
			onClick={onClick}
			type="button"
			disabled={disabled}
			className="dropdown-item"
			role="menuitem"
		>
			{children}
		</button>
	);
};

export default DropdownItem;
