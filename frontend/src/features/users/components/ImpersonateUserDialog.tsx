import { forwardRef, useImperativeHandle, useState } from "react";
import { ConfirmDialog } from "#/components/ui";
import { useImpersonateUser } from "../pages/hooks";
import type { ImpersonateUserFormCommand } from "./form/UserFormCommand";

type Target = { id: string; fullName?: string | null };

/**
 * Confirms signing in as somebody else. Worth a dialog rather than a straight click, because it
 * ends the administrator's own session: there is no way back from inside the application, and the
 * wording says so rather than leaving it to be discovered.
 *
 * Driven by a ref because two unrelated screens open it - the user list and the org chart - and one
 * mounted instance per screen is cheaper than two copies of this wording to keep in step.
 */
const ImpersonateUserDialog = forwardRef<ImpersonateUserFormCommand>((_props, ref) => {
	const [target, setTarget] = useState<Target | null>(null);

	const mutation = useImpersonateUser();

	useImperativeHandle(
		ref,
		() => ({
			impersonate: (next: Target) => {
				setTarget(next);
			},
		}),
		[],
	);

	if (!target) return null;

	return (
		<ConfirmDialog
			open={true}
			danger
			title={`Log in as ${target.fullName ?? "this person"}?`}
			description={
				"You will be signed in as them for the next 30 minutes and see exactly what they see - " +
				"useful for checking something like an approval that follows from the org chart rather " +
				"than from a role. Your own session ends the moment you confirm, and there is no way " +
				"back to it from inside the application: to return to your own account, sign out and " +
				"sign in again."
			}
			confirmLabel="Log in as them"
			loading={mutation.isPending}
			onConfirm={() => mutation.mutate(target.id)}
			onClose={() => setTarget(null)}
		/>
	);
});

ImpersonateUserDialog.displayName = "ImpersonateUserDialog";

export default ImpersonateUserDialog;
