import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import { TeamRole } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useChangeTeamMemberRole } from "../../pages/hooks";
import { teamRoles } from "../../types";

export interface ChangeMemberRoleFormCommand {
	changeRole(target: ChangeRoleTarget): void;
}

export type ChangeRoleTarget = {
	teamId: string;
	userId: string;
	memberName: string;
	currentRole: TeamRole;
};

interface ChangeMemberRoleDrawerProps {
	onSuccess: () => void;
}

const changeRoleSchema = z.object({
	role: z.enum(TeamRole),
});

const FormContent: React.FC<{
	target: ChangeRoleTarget;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ target, onSuccess, handleClose }) => {
	const { mutation, waiting } = useChangeTeamMemberRole({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: { role: target.currentRole },

		validators: {
			onChange: changeRoleSchema,
		},

		onSubmit: async ({ value }) => {
			mutation.mutate({
				teamId: target.teamId,
				userId: target.userId,
				request: { role: value.role },
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`Team role of ${target.memberName}`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField name="role">
							{(field) => (
								<field.FormSelectEnum
									label="Team role"
									options={teamRoles}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<div className="form-hint">
							This is what the person does on the team. It does not change what they may do in the
							organization.
						</div>

						<ApiError
							error={mutation.error as unknown as Parameters<typeof ApiError>[0]["error"]}
						/>
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					<form.FormSaveChangesButton wait={waiting} isPending={mutation.isPending} />
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
};

const ChangeMemberRoleDrawer = forwardRef<ChangeMemberRoleFormCommand, ChangeMemberRoleDrawerProps>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<ChangeRoleTarget | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				changeRole: (next: ChangeRoleTarget) => {
					setTarget(next);
				},
			}),
			[],
		);

		if (!target) return null;

		return (
			<FormContent target={target} onSuccess={onSuccess} handleClose={() => setTarget(null)} />
		);
	},
);

ChangeMemberRoleDrawer.displayName = "ChangeMemberRoleDrawer";

export default ChangeMemberRoleDrawer;
