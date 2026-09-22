import { forwardRef, useImperativeHandle, useState } from "react";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import type { CreateTeamRequest } from "@/api/models";
import { useCreateTeam } from "../../pages/hooks";
import { createEmptyTeam, TeamForm, teamSchema } from "./TeamForm";
import type { CreateTeamFormCommand } from "./TeamFormCommand";

interface CreateTeamDrawerProps {
	onSuccess: () => void;
}

const FormContent: React.FC<{ onSuccess: () => void; handleClose: () => void }> = ({
	onSuccess,
	handleClose,
}) => {
	const { mutation, waiting } = useCreateTeam({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: createEmptyTeam(),

		validators: {
			onChange: teamSchema,
		},

		onSubmit: async ({ value }) => {
			const request: CreateTeamRequest = {
				name: value.name.trim(),
				members: value.members.map(({ userId, role }) => ({ userId, role })),
			};

			mutation.mutate({ request });
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Create team"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<TeamForm form={form} error={mutation.error} isSubmitting={mutation.isPending} />
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					<form.FormSaveChangesButton wait={waiting} isPending={mutation.isPending} />
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
};

const CreateTeamDrawer = forwardRef<CreateTeamFormCommand, CreateTeamDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);

		useImperativeHandle(
			ref,
			() => ({
				create: () => {
					setIsOpen(true);
				},
			}),
			[],
		);

		if (!isOpen) return null;

		return <FormContent onSuccess={onSuccess} handleClose={() => setIsOpen(false)} />;
	},
);

CreateTeamDrawer.displayName = "CreateTeamDrawer";

export default CreateTeamDrawer;
