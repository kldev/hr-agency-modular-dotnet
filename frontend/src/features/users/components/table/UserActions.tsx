import { useNavigate } from "@tanstack/react-router";
import { Pencil, Settings2, ShieldCheck, UsersRound } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface UserActionsProps {
	id: string;
	email: string;
	onEdit: () => void;
	onChangeRole: () => void;
	onChangeTeam: () => void;
	mode?: "table" | "details";
}

export function UserActions({
	id,
	email,
	onEdit,
	onChangeRole,
	onChangeTeam,
	mode = "table",
}: UserActionsProps) {
	const navigate = useNavigate();

	const actions = [
		{ label: "Edit", icon: Pencil, action: onEdit },
		{ label: "Change role", icon: ShieldCheck, action: onChangeRole },
		{ label: "Change team", icon: UsersRound, action: onChangeTeam },
	];

	if (mode === "table") {
		actions.push({
			label: "Open details",
			icon: Settings2,
			action: () => {
				navigate({ to: "/app/users/$id", params: { id }, search: {} });
			},
		});
	}

	return (
		<div className="table-actions">
			<ActionMenu actions={actions} ariaLabel={`Actions for ${email}`} />
		</div>
	);
}
