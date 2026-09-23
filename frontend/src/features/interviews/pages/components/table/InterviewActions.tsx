import { useNavigate } from "@tanstack/react-router";
import { MapPinMinus, PersonStanding, TimerReset, TrendingUp, Users2 } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";
import type { InterviewActionsType } from "../forms";

interface InterviewActionsProps {
	onAction: (action: InterviewActionsType) => void;
	applicationId: string;
}
export function InterviewActions({ onAction, applicationId }: InterviewActionsProps) {
	const naviage = useNavigate();

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
					},
					{
						label: "Reschedule",
						icon: TimerReset,
						action: () => onAction("reschedule"),
						dividerAfter: true,
					},
					{
						label: "Show applicant info",
						icon: Users2,

						action: () => {
							naviage({
								to: "/app/applications/$id",
								search: { search: undefined, source: undefined, status: undefined, tab: undefined },
								params: { id: applicationId },
							});
						},
					},
				]}
			/>
		</div>
	);
}
