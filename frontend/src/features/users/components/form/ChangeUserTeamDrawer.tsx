import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import { TeamRole, type UserProjection } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { teamRoles } from "#/features/teams/types";
import { useAppForm } from "#/forms";
import { useTeamSuggestion } from "#/hooks";
import { useChangeUserTeam } from "../../pages/hooks";
import type { ChangeUserTeamFormCommand } from "./UserFormCommand";

interface ChangeUserTeamDrawerProps {
	onSuccess: () => void;
}

const changeTeamSchema = z.object({
	teamId: z.string(),
	role: z.enum(TeamRole),
});

const FormContent: React.FC<{
	user: UserProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ user, onSuccess, handleClose }) => {
	const current = user.team ?? null;

	/*
	 * The roster size of the team the person is leaving. The domain refuses to empty a team, so
	 * moving or unassigning its last member is impossible - better said here than discovered as a
	 * 400 after pressing save.
	 */
	const { data: currentTeam } = useTeamSuggestion(current?.id ?? "");
	const isLastMember = currentTeam !== undefined && Number(currentTeam.memberCount) <= 1;

	const blocksTheMove = (teamId: string) =>
		Boolean(isLastMember && current && teamId !== current.id);

	const { mutation, waiting, detached } = useChangeUserTeam({
		onSuccess: () => {
			mutation.reset();
			toast.success("Team updated");
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: {
			teamId: current?.id ?? "",
			role: current?.role ?? TeamRole.Recruiter,
		},

		validators: {
			onChange: changeTeamSchema,
		},

		onSubmit: async ({ value }) => {
			mutation.mutate({
				userId: user.id,
				current,
				teamId: value.teamId || null,
				role: value.role,
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`Team of ${user.fullName ?? user.email}`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField name="teamId">
							{(field) => (
								<field.FormTeamPicker
									label="Team"
									placeholder="Search teams ..."
									hint={
										current
											? `Currently on ${current.name}. Clearing the field takes them off the team.`
											: "Currently on no team."
									}
									fieldValue={{ id: field.state.value }}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val.id ?? "")}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.Subscribe selector={(state) => state.values.teamId}>
							{(teamId) =>
								teamId ? (
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
								) : null
							}
						</form.Subscribe>

						<form.Subscribe selector={(state) => state.values.teamId}>
							{(teamId) =>
								blocksTheMove(teamId) && current ? (
									<div className="form-error" role="alert">
										<strong>
											{user.fullName ?? user.email} is the last member of {current.name}.
										</strong>
										<div className="text-xl text-muted!">
											A team always keeps at least one member, so they cannot leave it. Add somebody
											else to {current.name} first.
										</div>
									</div>
								) : null
							}
						</form.Subscribe>

						{detached ? (
							<div className="form-error" role="alert">
								<strong>Removed from {current?.name}, but not added to the new team.</strong>
								<div className="text-xl text-muted!">
									Moving somebody takes two steps and the second one failed, so this person is
									currently on no team. Pick a team again to finish the move.
								</div>
							</div>
						) : null}

						<ApiError
							error={mutation.error as unknown as Parameters<typeof ApiError>[0]["error"]}
						/>
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					<form.Subscribe selector={(state) => state.values.teamId}>
						{(teamId) => (
							// isPending is the only way to disable FormSaveChangesButton, so a move the domain
							// would refuse is fed through it rather than left clickable.
							<form.FormSaveChangesButton
								wait={waiting}
								isPending={mutation.isPending || blocksTheMove(teamId)}
							/>
						)}
					</form.Subscribe>
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
};

const ChangeUserTeamDrawer = forwardRef<ChangeUserTeamFormCommand, ChangeUserTeamDrawerProps>(
	({ onSuccess }, ref) => {
		const [user, setUser] = useState<UserProjection | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				changeTeam: (next: UserProjection) => {
					setUser(next);
				},
			}),
			[],
		);

		if (!user) return null;

		return <FormContent user={user} onSuccess={onSuccess} handleClose={() => setUser(null)} />;
	},
);

ChangeUserTeamDrawer.displayName = "ChangeUserTeamDrawer";

export default ChangeUserTeamDrawer;
