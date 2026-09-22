import { useNavigate } from "@tanstack/react-router";
import { CalendarOff, Pencil, User } from "lucide-react";
import type { ActionMenuItem } from "@/components/ui/ActionMenu";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface Props {
	userId: string;
	name: string;
	isEnded: boolean;
	onChangeTerms: () => void;
	onEnd: () => void;
	mode?: "table" | "details";
}

export function EmploymentActions({
	userId,
	name,
	isEnded,
	onChangeTerms,
	onEnd,
	mode = "table",
}: Props) {
	const navigate = useNavigate();

	const actions: ActionMenuItem[] = [];

	/*
	 * Both refusals share one message on the backend ("this engagement has ended"), so both
	 * actions go at once: history is read, not edited.
	 */
	if (!isEnded) {
		actions.push({ label: "Change terms", icon: Pencil, action: onChangeTerms });
		actions.push({ label: "End engagement", icon: CalendarOff, action: onEnd });
	}

	if (mode === "table") {
		actions.push({
			label: "Open person",
			icon: User,
			action: () => {
				navigate({ to: "/app/users/$id", params: { id: userId } });
			},
		});
	}

	return (
		<div className="table-actions">
			<ActionMenu actions={actions} ariaLabel={`Actions for ${name}`} />
		</div>
	);
}
