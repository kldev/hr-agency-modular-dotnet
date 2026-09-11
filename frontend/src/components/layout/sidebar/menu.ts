import {
	BriefcaseBusiness,
	Building2,
	CalendarDays,
	ClipboardList,
	DollarSign,
	LayoutDashboard,
	MessageSquare,
	Users,
} from "lucide-react";
import { ROUTES } from "@/routes";
import type { MenuGroup } from "./types/sidebar";

const menuGroups: MenuGroup[] = [
	{
		title: "",
		items: [{ label: "Dashboard", icon: LayoutDashboard, link: ROUTES.DASHBOARD }],
	},
	{
		title: "Recruitment",
		items: [
			{ label: "Job postings", icon: BriefcaseBusiness, link: ROUTES.JOBS },
			{ label: "Candidates", icon: Users, link: ROUTES.CANDIDATES },
			{ label: "Applications", icon: ClipboardList, link: ROUTES.APPLICATIONS },
			{ label: "Interviews", icon: MessageSquare, link: ROUTES.INTERVIEWS },
			{ label: "Calendar", icon: CalendarDays, link: ROUTES.CALENDAR },
		],
	},
	{
		title: "Sales",
		items: [
			{ label: "Companies", icon: Building2, link: ROUTES.COMPANIES },
			{ label: "Sales", icon: DollarSign, link: ROUTES.SALES },
		],
	},
	{
		title: "Users",
		items: [{ label: "Users", icon: Users, link: ROUTES.USERS }],
	},
];

export { menuGroups };
