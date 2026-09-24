import {
	BriefcaseBusiness,
	Building2,
	CalendarClock,
	CalendarDays,
	ChessRook,
	ClipboardCheck,
	ClipboardList,
	DollarSign,
	FileBadge,
	FileText,
	FolderKanban,
	HardHat,
	Landmark,
	LayoutDashboard,
	MessageSquare,
	Network,
	PanelsTopLeft,
	Plane,
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
			{ label: "Sales workspace", icon: PanelsTopLeft, link: "/app/sales-workspace" },
		],
	},
	{
		title: "Delivery",
		items: [
			{ label: "Projects", icon: FolderKanban, link: "/app/projects" },
			{ label: "Positions", icon: BriefcaseBusiness, link: "/app/positions" },
			{ label: "Workers", icon: HardHat, link: "/app/workers" },
			{ label: "Workers abroad", icon: Plane, link: "/app/workers-abroad" },
			{ label: "Assignments", icon: ClipboardCheck, link: "/app/assignments" },
			{ label: "Forms", icon: FileText, link: "/app/forms" },
		],
	},
	{
		title: "Organization",
		items: [
			{ label: "Users", icon: Users, link: "/app/users" },
			{ label: "Teams", icon: UsersRound, link: "/app/teams" },
			{ label: "Structure", icon: Network, link: "/app/org-structure" },
			{ label: "Employment", icon: FileBadge, link: "/app/employment" },
			{ label: "Time sheets", icon: CalendarClock, link: "/app/timesheets" },
			{ label: "Legal entities", icon: Landmark, link: "/app/legal-entities" },
		],
	},
];

export { menuGroups };
