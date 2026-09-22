import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import type { ProjectProjection } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useAssignProjectTeam } from "../pages/hooks";
import type { AssignProjectTeamFormCommand } from "./ProjectFormCommand";

interface AssignProjectTeamDrawerProps {
	onSuccess: () => void;
}

const teamSchema = z.object({
	teamId: z.string().min(1, "Pick a team"),
});

const FormContent: React.FC<{
	project: ProjectProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ project, onSuccess, handleClose }) => {
	const { mutation, waiting } = useAssignProjectTeam({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: {
			teamId: project.teamId ?? "",
		},

		validators: {
			onChange: teamSchema,
		},

		onSubmit: async ({ value }) => {
			mutation.mutate({ projectId: project.id, request: { teamId: value.teamId } });
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Assign team"
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
									fieldName={field.name}
									fieldValue={{ id: field.state.value || null }}
									errors={field.state.meta.errors}
									handleChange={(value) => field.handleChange(value.id ?? "")}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<div className="form-hint">
							A project runs with one team. Assigning another replaces it; there is no command for
							leaving a project without a team.
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

const AssignProjectTeamDrawer = forwardRef<
	AssignProjectTeamFormCommand,
	AssignProjectTeamDrawerProps
>(({ onSuccess }, ref) => {
	const [project, setProject] = useState<ProjectProjection | null>(null);

	useImperativeHandle(
		ref,
		() => ({
			assignTeam: (target: ProjectProjection) => {
				setProject(target);
			},
		}),
		[],
	);

	if (!project) return null;

	return (
		<FormContent project={project} onSuccess={onSuccess} handleClose={() => setProject(null)} />
	);
});

AssignProjectTeamDrawer.displayName = "AssignProjectTeamDrawer";

export default AssignProjectTeamDrawer;
