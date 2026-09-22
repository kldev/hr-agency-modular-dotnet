import { useNavigate } from "@tanstack/react-router";
import { Image, LogIn, Pencil, Settings2, ShieldCheck, UsersRound } from "lucide-react";
import { ActionMenu, type ActionMenuItem } from "@/components/ui/ActionMenu";
import { useAuthStore } from "@/stores/authStore";
import { isAdmin } from "../../types";

interface UserActionsProps {
	id: string;
	email: string;
	onEdit: () => void;
	onChangeRole: () => void;
	onChangeTeam: () => void;
	onSetAvatar: () => void;
	onImpersonate: () => void;
	mode?: "table" | "details";
}

export function UserActions({
	id,
	email,
	onEdit,
	onChangeRole,
	onChangeTeam,
	onSetAvatar,
	onImpersonate,
	mode = "table",
}: UserActionsProps) {
	const navigate = useNavigate();
	const currentUser = useAuthStore((state) => state.user);

	const administrator = isAdmin(currentUser?.role);
	const isSelf = currentUser?.userId === id;

	const actions: ActionMenuItem[] = [
		{ label: "Edit", icon: Pencil, action: onEdit },
		{ label: "Change role", icon: ShieldCheck, action: onChangeRole },
		{ label: "Change team", icon: UsersRound, action: onChangeTeam },
	];

	/*
	 * Absent rather than greyed out for anybody who is not an administrator: these are not things
	 * they could do under some other condition. Signing in as yourself is the exception - it stays
	 * visible and disabled, because on your own row the reason is worth saying.
	 */
	if (administrator) {
		actions.push({ label: "Set picture", icon: Image, action: onSetAvatar });
		actions.push({
			label: "Log in as",
			icon: LogIn,
			action: onImpersonate,
			disabled: isSelf,
			hint: isSelf ? "You are already signed in as yourself." : undefined,
		});
	}

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
