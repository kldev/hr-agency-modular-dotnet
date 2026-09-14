import { BarChart3, Globe2, LayoutDashboard, Settings, Users } from "lucide-react";

import type { MenuGroup } from "./types/sidebar";

const ownerMenu: MenuGroup[] = [
	{
		title: "",
		items: [{ label: "Dashboard", icon: LayoutDashboard, link: "/admin/dashboard" }],
	},
	{
		title: "System",
		items: [
			{ label: "Organizations", icon: Globe2, link: "/admin/organizations" },
			{ label: "Users", icon: Users, link: "/admin/users" },
			{ label: "Reports", icon: BarChart3, link: "/404" },
			{ label: "Settings", icon: Settings, link: "/404" },
		],
	},
];

export { ownerMenu };
