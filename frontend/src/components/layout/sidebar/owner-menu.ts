import { BarChart3, Globe2, LayoutDashboard, Settings } from "lucide-react";
import { OWNER_ROUTES } from "@/routes/OwnerRoutes";
import type { MenuGroup } from "./types/sidebar";

const ownerMenu: MenuGroup[] = [
	{
		title: "",
		items: [{ label: "Dashboard", icon: LayoutDashboard, link: OWNER_ROUTES.DASHBOARD }],
	},
	{
		title: "System",
		items: [
			{ label: "Organizations", icon: Globe2, link: OWNER_ROUTES.ORGANIZATIONS },
			{ label: "Reports", icon: BarChart3, link: OWNER_ROUTES.REPORTS },
			{ label: "Settings", icon: Settings, link: OWNER_ROUTES.SETTINGS },
		],
	},
];

export { ownerMenu };
