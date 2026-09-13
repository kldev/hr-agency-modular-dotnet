import { MapPinMinus, PersonStanding, TimerReset, TrendingUp } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";
import type { InterviewActionsType } from "../forms";

interface InterviewActionsProps {
	onAction: (action: InterviewActionsType) => void;
}
export function InterviewActions({ onAction }: InterviewActionsProps) {
	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{
						label: "Change interviewer",
						icon: PersonStanding,
						action: () => onAction("change-Interviewer"),
					},
					{ label: "Change status", icon: TrendingUp, action: () => onAction("change-status") },
					{
						label: "Change format",
						icon: MapPinMinus,
						action: () => onAction("change-format"),
						dividerAfter: true,
					},
					{ label: "Reschedule", icon: TimerReset, action: () => onAction("reschedule") },
				]}
			/>
		</div>
	);
}
