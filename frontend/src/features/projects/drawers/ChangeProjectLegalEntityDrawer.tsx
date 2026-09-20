import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import type { ProjectProjection } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { LegalEntitySelect } from "#/features/legal-entities/pages/components";
import { useAppForm } from "#/forms";
import { useChangeProjectLegalEntity } from "../pages/hooks";

export interface ChangeProjectLegalEntityCommand {
	change: (project: ProjectProjection) => void;
}

const schema = z.object({
	legalEntityId: z.string().min(1, "Pick the company that will deliver the project"),
});

const FormContent: React.FC<{
	project: ProjectProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ project, onSuccess, handleClose }) => {
	const { mutation, waiting } = useChangeProjectLegalEntity({
		onSuccess: () => {
			mutation.reset();
			toast.success("Delivering company changed");
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: { legalEntityId: project.deliveringEntity.legalEntityId },

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			mutation.mutate({ projectId: project.id, legalEntityId: value.legalEntityId });
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Change the delivering company"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<p className="form-hint">
							Possible only while the project is a draft. Once it starts, its contract and its
							notifications name this company, and carrying on elsewhere is a new project.
						</p>

						<form.AppField name="legalEntityId">
							{(field) => (
								<LegalEntitySelect
									label="Our company"
									fieldName={field.name}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									handleChange={(value) => field.handleChange(value)}
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

export const ChangeProjectLegalEntityDrawer = forwardRef<
	ChangeProjectLegalEntityCommand,
	{ onSuccess: () => void }
>(({ onSuccess }, ref) => {
	const [project, setProject] = useState<ProjectProjection | null>(null);

	useImperativeHandle(ref, () => ({ change: setProject }), []);

	if (!project) return null;

	return (
		<FormContent project={project} onSuccess={onSuccess} handleClose={() => setProject(null)} />
	);
});

ChangeProjectLegalEntityDrawer.displayName = "ChangeProjectLegalEntityDrawer";
