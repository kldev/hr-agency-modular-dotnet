import type { LinkProps } from "@tanstack/react-router";
import type { LucideIcon } from "lucide-react";

export interface MenuItem {
	label: string;
	icon: LucideIcon;
	link: LinkProps["to"];
}

export interface MenuGroup {
	title: string;
	items: MenuItem[];
}
