import {
	CalendarDays,
	CircleCheck,
	type LucideIcon,
	Mail,
	MessageSquare,
	Phone,
	Presentation,
} from "lucide-react";
import type { SalesActivityType } from "#/api/models";

export const activityIcons: Record<SalesActivityType, LucideIcon> = {
	Call: Phone,
	Email: Mail,
	Meeting: CalendarDays,
	Note: MessageSquare,
	Presentation: Presentation,
	Other: MessageSquare,
	Task: CircleCheck,
};
