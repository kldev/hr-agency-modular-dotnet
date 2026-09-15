import { useNavigate } from "@tanstack/react-router";
import { ClockArrowRight, Pencil, Settings2 } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";
import type { SalesActionTypes } from "../forms";

interface SalesActionsProps {
	onAction: (action: SalesActionTypes) => void;
	opportunityId: string;
	mode: "table" | "details";
}
export function SalesActions({ onAction, opportunityId }: SalesActionsProps) {
	const naviage = useNavigate();

	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{
						label: "Log activity",
						icon: ClockArrowRight,
						action: () => onAction("log-activity"),
					},
					{ label: "Edit", icon: Pencil, action: () => onAction("edit-opportunity") },

					{
						label: "Open details",
						icon: Settings2,

						action: () => {
							naviage({
								to: "/app/sales/opportunities/$id",
								search: { search: undefined, source: undefined, status: undefined },
								params: { id: opportunityId },
							});
						},
					},
				]}
			/>
		</div>
	);
}
