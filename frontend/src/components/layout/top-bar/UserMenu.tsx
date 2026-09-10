import { LogOut, Moon, Settings2, Sun, User } from "lucide-react";
import type React from "react";
import { useNavigate } from "react-router-dom";
import Dropdown from "@/components/ui/dropdown/Dropdown";
import DropdownDivider from "@/components/ui/dropdown/DropdownDivider";
import DropdownItem from "@/components/ui/dropdown/DropdownItem";
import { useAuthStore } from "@/stores/authStore";
import { useUiStore } from "@/stores/uiStore";

const UserMenu: React.FC = () => {
	const ui = useUiStore();
	const store = useAuthStore();

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
			<DropdownItem
				onClick={() => {
					const theme = ui.getMode();
					if (theme === "light") ui.setMode("dark");
					else ui.setMode("light");
				}}
			>
				{ui.getMode() === "dark" ? <Sun size={15} /> : <Moon size={15} />}
				{ui.getMode() === "dark" ? "Light theme" : "Dark theme"}
			</DropdownItem>

			<DropdownDivider />

			<DropdownItem
				onClick={() => {
					store.clearUser();
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
