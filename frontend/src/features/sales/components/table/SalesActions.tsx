import { useNavigate } from "@tanstack/react-router";
import { CalendarClock, ClockArrowRight, Pencil, Settings2, TrendingUp } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";
import type { SalesActionTypes } from "../forms";

interface SalesActionsProps {
	onAction: (action: SalesActionTypes) => void;
	opportunityId: string;
	mode: "table" | "details";
}
export function SalesActions({ onAction, opportunityId, mode }: SalesActionsProps) {
	const navigate = useNavigate();

	const actions = [
		{
			label: "Log activity",
			icon: ClockArrowRight,
			action: () => onAction("log-activity"),
		},
		{
			label: "Change stage",
			icon: TrendingUp,
			action: () => onAction("change-stage"),
		},
		{
			label: "Add follow up",
			icon: CalendarClock,
			action: () => onAction("add-follow-up"),
		},
	];

	if (mode === "table") {
		actions.push(
			{ label: "Edit", icon: Pencil, action: () => onAction("edit-opportunity") },

			{
				label: "Open details",
				icon: Settings2,

				action: () => {
					navigate({
						to: "/app/sales/opportunities/$id",
						params: { id: opportunityId },
					});
				},
			},
		);
	}

	return (
		<div className="table-actions">
			<ActionMenu actions={actions} />
		</div>
	);
}
