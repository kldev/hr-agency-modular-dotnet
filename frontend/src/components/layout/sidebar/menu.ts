import {
	BarChart3,
	BriefcaseBusiness,
	Building2,
	CalendarDays,
	ClipboardList,
	DollarSign,
	Globe2,
	LayoutDashboard,
	MessageSquare,
	Settings,
	Users,
} from "lucide-react";
import type { MenuGroup } from "./types/sidebar";

const menuGroups: MenuGroup[] = [
	{
		title: "",
		items: [{ label: "Dashboard", icon: LayoutDashboard, link: "/" }],
	},
	{
		title: "Recruitment",
		items: [
			{ label: "Job postings", icon: BriefcaseBusiness, link: "/jobs" },
			{ label: "Candidates", icon: Users, link: "/candidates" },
			{ label: "Applications", icon: ClipboardList, link: "/applications" },
			{ label: "Interviews", icon: MessageSquare, link: "/interviews" },
			{ label: "Calendar", icon: CalendarDays, link: "/calendar" },
		],
	},
	{
		title: "Sales",
		items: [
			{ label: "Companies", icon: Building2, link: "/companies" },
			{ label: "Sales", icon: DollarSign, link: "/sales" },
		],
	},
	{
		title: "Users",
		items: [{ label: "Users", icon: Users, link: "/users" }],
	},
	{
		title: "System",
		items: [
			{ label: "Organizations", icon: Globe2, link: "/organizations" },
			{ label: "Reports", icon: BarChart3, link: "/reports" },
			{ label: "Settings", icon: Settings, link: "/settings" },
		],
	},
];

export { menuGroups };
