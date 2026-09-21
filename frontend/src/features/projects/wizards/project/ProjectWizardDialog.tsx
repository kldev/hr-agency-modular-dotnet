import { forwardRef, useImperativeHandle, useState } from "react";
import type { ProjectProjection } from "#/api/models";
import { ConfirmDialog, Dialog, useUnsavedChangesGuard } from "#/components/ui";
import { ProjectWizard } from "./ProjectWizard";
import { emptyProject, type ProjectFormValues } from "./schema";

export interface ProjectWizardCommand {
	create: () => void;
	edit: (project: ProjectProjection) => void;
}

interface ProjectWizardDialogProps {
	onSuccess: (projectId: string) => void;
}

type Target =
	| { mode: "create"; projectId?: undefined; values: ProjectFormValues }
	| { mode: "edit"; projectId: string; values: ProjectFormValues };

/** The projection is the read model; the wizard works in strings, so nulls flatten to "". */
function toFormValues(project: ProjectProjection): ProjectFormValues {
	return {
		companyId: project.companyId,
		legalEntityId: project.deliveringEntity.legalEntityId,
		name: project.name,
		description: project.description,
		engagementType: project.engagementType,
		street: project.workplaceAddress.street,
		buildingNumber: project.workplaceAddress.buildingNumber,
		unitNumber: project.workplaceAddress.unitNumber ?? "",
		postalCode: project.workplaceAddress.postalCode,
		city: project.workplaceAddress.city,
		countryCode: project.workplaceAddress.countryCode,
		startsOn: project.startsOn,
		endsOn: project.endsOn ?? "",
		teamId: project.teamId ?? "",
	};
}

/**
 * The wizard lives in a dialog rather than on its own route, and that is the whole reason this
 * component exists: a dialog closes on a stray Escape or a click on the cross, where a route does
 * not. Anything already typed is therefore worth a question before it disappears.
 */
const ProjectWizardDialog = forwardRef<ProjectWizardCommand, ProjectWizardDialogProps>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<Target | null>(null);

		const { setDirty, confirming, requestClose, discard, keepEditing } = useUnsavedChangesGuard(
			() => setTarget(null),
		);

		useImperativeHandle(
			ref,
			() => ({
				create: () => {
					setDirty(false);
					setTarget({ mode: "create", values: emptyProject });
				},
				edit: (project: ProjectProjection) => {
					setDirty(false);
					setTarget({ mode: "edit", projectId: project.id, values: toFormValues(project) });
				},
			}),
			[setDirty],
		);

		if (!target) {
			return null;
		}

		const editing = target.mode === "edit";

		return (
			<>
				<Dialog
					open={true}
					title={editing ? "Edit project" : "New project"}
					maxWidth="wide"
					onClose={requestClose}
				>
					<ProjectWizard
						key={target.projectId ?? "create"}
						projectId={target.projectId}
						initialValues={target.values}
						onDirtyChange={setDirty}
						onCancel={requestClose}
						onSaved={(projectId) => {
							discard();
							onSuccess(projectId);
						}}
					/>
				</Dialog>

				<ConfirmDialog
					open={confirming}
					title={editing ? "Discard these changes?" : "Discard this project?"}
					description="Nothing has been saved yet. Closing now throws away everything filled in so far."
					confirmLabel="Discard"
					onConfirm={discard}
					onClose={keepEditing}
				/>
			</>
		);
	},
);

ProjectWizardDialog.displayName = "ProjectWizardDialog";

export { ProjectWizardDialog };
