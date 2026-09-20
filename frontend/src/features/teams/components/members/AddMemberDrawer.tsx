import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import { TeamRole } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useAddTeamMember } from "../../pages/hooks";
import { defaultTeamRole, teamRoles } from "../../types";

export interface AddMemberFormCommand {
	add(teamId: string, teamName: string): void;
}

interface AddMemberDrawerProps {
	onSuccess: () => void;
}

type AddTarget = {
	teamId: string;
	teamName: string;
};

const addMemberSchema = z.object({
	userId: z.string().trim().min(1, "Pick a person"),
	role: z.enum(TeamRole),
});

const FormContent: React.FC<{
	target: AddTarget;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ target, onSuccess, handleClose }) => {
	const { mutation, waiting } = useAddTeamMember({
		onSuccess: () => {
			mutation.reset();
			toast.success("Member added");
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: { userId: "", role: defaultTeamRole },

		validators: {
			onChange: addMemberSchema,
		},

		onSubmit: async ({ value }) => {
			mutation.mutate({
				teamId: target.teamId,
				request: { userId: value.userId, role: value.role },
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`Add to ${target.teamName}`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField name="userId">
							{(field) => (
								<field.FormUserPicker
									label="Person"
									placeholder="Search people ..."
									fieldValue={{ id: field.state.value }}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val.id ?? "")}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<div className="form-hint">
							A person belongs to at most one team. Somebody already on another team has to be moved
							from their profile instead of added here.
						</div>

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

const AddMemberDrawer = forwardRef<AddMemberFormCommand, AddMemberDrawerProps>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<AddTarget | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				add: (teamId: string, teamName: string) => {
					setTarget({ teamId, teamName });
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

AddMemberDrawer.displayName = "AddMemberDrawer";

export default AddMemberDrawer;
