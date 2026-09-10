import React from "react";
import Dropdown from "../../ui/Dropdown/Dropdown";
import { LogOut, Moon, Settings2, Sun, User } from "lucide-react";
import DropdownItem from "../../ui/Dropdown/DropdownItem";
import DropdownDivider from "../../ui/Dropdown/DropdownDivider";
import { useNavigate } from "react-router-dom";

interface Props {
	theme: "light" | "dark";
	onThemeChange: (theme: "light" | "dark") => void;
}

const UserMenu: React.FC<Props> = ({ theme, onThemeChange }) => {
	const navigation = useNavigate();
	return (
		<Dropdown>
			<DropdownItem>
				<User size={15} />
				Profile
			</DropdownItem>
			<DropdownItem>
				<Settings2 size={15} />
				Preferences
			</DropdownItem>

			<DropdownDivider />
			<DropdownItem onClick={() => onThemeChange(theme === "dark" ? "light" : "dark")}>
				{theme === "dark" ? <Sun size={15} /> : <Moon size={15} />}
				{theme === "dark" ? "Light theme" : "Dark theme"}
			</DropdownItem>

			<DropdownDivider />

			<DropdownItem
				onClick={() => {
					navigation("/login");
				}}
			>
				<LogOut size={15} />
				Sign out
			</DropdownItem>
		</Dropdown>
	);
};

export default UserMenu;
