import {
	CheckCircle2,
	CircleDollarSign,
	type LucideIcon,
	Phone,
	Target,
	Trophy,
	ViewIcon,
	XCircle,
} from "lucide-react";
import type { OpportunityStage } from "#/api/models";

// the same vocabulary the stage badge uses, so a column and a badge say the same thing
export const stageIcons: Record<OpportunityStage, LucideIcon> = {
	New: Target,
	Viewed: ViewIcon,
	Contacted: Phone,
	Qualified: CheckCircle2,
	Proposal: CircleDollarSign,
	Won: Trophy,
	Lost: XCircle,
};
