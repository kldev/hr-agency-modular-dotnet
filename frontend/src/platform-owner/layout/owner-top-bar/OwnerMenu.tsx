import { LogOut, Moon, Sun, User } from "lucide-react";
import type React from "react";
import { useNavigate } from "react-router-dom";
import Dropdown from "@/components/ui/dropdown/Dropdown";
import DropdownDivider from "@/components/ui/dropdown/DropdownDivider";
import DropdownItem from "@/components/ui/dropdown/DropdownItem";
import { useOwnerAuthStore } from "@/platform-owner/stores/authOwnerStore";
import { OWNER_ROUTES } from "@/routes/OwnerRoutes";
import { useUiStore } from "@/stores/uiStore";

const OwnerMenu: React.FC = () => {
	const ui = useUiStore();
	const store = useOwnerAuthStore();

	const navigation = useNavigate();
	return (
		<Dropdown>
			<DropdownItem>
				<User size={15} />
				Profile
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
					store.clear();
					navigation(OWNER_ROUTES.LOGIN);
				}}
			>
				<LogOut size={15} />
				Sign out
			</DropdownItem>
		</Dropdown>
	);
};

export default OwnerMenu;
