import type { LucideIcon } from "lucide-react";

export interface MenuItem {
	label: string;
	icon: LucideIcon;
	link: string;
}

export interface MenuGroup {
	title: string;
	items: MenuItem[];
}
