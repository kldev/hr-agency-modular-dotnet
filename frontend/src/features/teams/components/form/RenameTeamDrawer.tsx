import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useRenameTeam } from "../../pages/hooks";
import type { RenameTeamFormCommand } from "./TeamFormCommand";

interface RenameTeamDrawerProps {
	onSuccess: () => void;
}

type RenameTarget = {
	teamId: string;
	currentName: string;
};

const renameSchema = z.object({
	name: z.string().trim().min(1, "Name is required").max(100, "Name cannot exceed 100 characters."),
});

const FormContent: React.FC<{
	target: RenameTarget;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ target, onSuccess, handleClose }) => {
	const { mutation, waiting } = useRenameTeam({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: { name: target.currentName },

		validators: {
			onChange: renameSchema,
		},

		onSubmit: async ({ value }) => {
			mutation.mutate({ teamId: target.teamId, request: { name: value.name.trim() } });
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Rename team"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField name="name">
							{(field) => (
								<field.FormInput
									label="Name"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<div className="form-hint">
							The new name follows the team everywhere, including on the profile of everyone on it.
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

const RenameTeamDrawer = forwardRef<RenameTeamFormCommand, RenameTeamDrawerProps>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<RenameTarget | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				rename: (teamId: string, currentName: string) => {
					setTarget({ teamId, currentName });
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

RenameTeamDrawer.displayName = "RenameTeamDrawer";

export default RenameTeamDrawer;
