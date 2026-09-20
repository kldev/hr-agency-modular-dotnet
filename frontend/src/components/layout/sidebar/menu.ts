import {
	BriefcaseBusiness,
	Building2,
	CalendarDays,
	ChessRook,
	ClipboardList,
	DollarSign,
	FolderKanban,
	Landmark,
	LayoutDashboard,
	MessageSquare,
	Users,
	UsersRound,
} from "lucide-react";
import type { MenuGroup } from "./types/sidebar";

const menuGroups: MenuGroup[] = [
	{
		title: "",
		items: [{ label: "Dashboard", icon: LayoutDashboard, link: "/app/dashboard" }],
	},
	{
		title: "Recruitment",
		items: [
			{ label: "Job postings", icon: BriefcaseBusiness, link: "/app/jobs" },
			{ label: "Candidates", icon: Users, link: "/app/candidates" },
			{ label: "Applications", icon: ClipboardList, link: "/app/applications" },
			{ label: "Interviews", icon: MessageSquare, link: "/app/interviews" },
			{ label: "Calendar", icon: CalendarDays, link: "/app/calendar" },
		],
	},
	{
		title: "Sales",
		items: [
			{ label: "Job descriptions", icon: ChessRook, link: "/app/job-descriptions" },
			{ label: "Companies", icon: Building2, link: "/app/companies" },
			{ label: "Sales", icon: DollarSign, link: "/app/sales" },
		],
	},
	{
		title: "Delivery",
		items: [{ label: "Projects", icon: FolderKanban, link: "/app/projects" }],
	},
	{
		title: "Organization",
		items: [
			{ label: "Users", icon: Users, link: "/app/users" },
			{ label: "Teams", icon: UsersRound, link: "/app/teams" },
			{ label: "Legal entities", icon: Landmark, link: "/app/legal-entities" },
		],
	},
];

export { menuGroups };
