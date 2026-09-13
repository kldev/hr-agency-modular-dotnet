import { MapPinMinus, PersonStanding, TrendingUp } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface InterviewActionsProps {
	onChangeForamt: () => void;
	onChangeStatus: () => void;
	onChangeInterviewer: () => void;
}
export function InterviewActions({
	onChangeForamt,
	onChangeStatus,
	onChangeInterviewer,
}: InterviewActionsProps) {
	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{
						label: "Change interviewer",
						icon: PersonStanding,
						action: onChangeInterviewer,
					},
					{ label: "Change status", icon: TrendingUp, action: onChangeStatus },
					{ label: "Change format", icon: MapPinMinus, action: onChangeForamt },
				]}
			/>
		</div>
	);
}
