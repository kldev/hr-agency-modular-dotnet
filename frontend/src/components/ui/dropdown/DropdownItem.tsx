import type React from "react";

interface Props {
	children: React.ReactNode;
	onClick?: () => void;
	disabled?: boolean;
	/** Why the action is closed, for the cases where a greyed-out entry would otherwise puzzle. */
	title?: string;
}

const DropdownItem: React.FC<Props> = ({ children, onClick, disabled, title }) => {
	return (
		<button
			onClick={onClick}
			type="button"
			disabled={disabled}
			title={title}
			className="dropdown-item"
			role="menuitem"
		>
			{children}
		</button>
	);
};

export default DropdownItem;
