import { forwardRef, useImperativeHandle, useState } from "react";
import type { ProjectPositionProjection } from "#/api/models";
import { ConfirmDialog, Dialog, useUnsavedChangesGuard } from "#/components/ui";
import { useGetPosition } from "../../pages/hooks";
import { PositionWizard } from "./PositionWizard";
import { emptyPosition, type PositionFormValues } from "./schema";

export interface PositionWizardCommand {
	/** Opened from a project the role belongs to, or from the register with no project in hand. */
	open: (projectId?: string) => void;
	edit: (positionId: string) => void;
}

interface PositionWizardDialogProps {
	onSuccess: () => void;
}

type Target =
	| { mode: "open"; projectId?: string; positionId?: undefined }
	| { mode: "edit"; positionId: string };

/** The projection is the read model; the wizard works in strings, so nulls flatten to "". */
function toFormValues(position: ProjectPositionProjection): PositionFormValues {
	return {
		projectId: position.projectId,
		name: position.name,
		contractName: position.contractName,
		defaultEngagementType: position.defaultEngagementType ?? "",
		plannedHeadcount: position.plannedHeadcount === null ? "" : String(position.plannedHeadcount),
		workDescription: position.workDescription,
		duties: position.duties,
		requiredQualifications: position.requiredQualifications,
		contractType: position.contractType,
		rateAmount: position.proposedRate ? String(position.proposedRate.amount) : "",
		rateCurrency: position.proposedRate?.currency ?? "",
		rateUnit: position.proposedRate?.unit ?? "Hourly",
		rateBasis: position.proposedRate?.basis ?? "Gross",
		payoutDay: position.payoutDay === null ? "" : String(position.payoutDay),
		probationPeriod: position.probationPeriod,
		noticePeriod: position.noticePeriod,
		allowances: position.allowances,
		weeklyHours: position.weeklyHours === null ? "" : String(position.weeklyHours),
		workStartsAt: position.workStartsAt ? position.workStartsAt.slice(0, 5) : "",
		workSchedule: position.workSchedule,
		street: position.workplaceAddress?.street ?? "",
		buildingNumber: position.workplaceAddress?.buildingNumber ?? "",
		unitNumber: position.workplaceAddress?.unitNumber ?? "",
		postalCode: position.workplaceAddress?.postalCode ?? "",
		city: position.workplaceAddress?.city ?? "",
		countryCode: position.workplaceAddress?.countryCode ?? "",
	};
}

/**
 * Editing mounts the wizard only once the role has been read, because `useAppForm` takes its
 * default values on the first render — a form mounted empty and filled by an effect keeps the
 * empty ones.
 */
function EditContent({
	positionId,
	onDirtyChange,
	onCancel,
	onSaved,
}: {
	positionId: string;
	onDirtyChange: (dirty: boolean) => void;
	onCancel: () => void;
	onSaved: () => void;
}) {
	const query = useGetPosition(positionId);

	if (query.isLoading) {
		return <p className="form-hint">Loading the role…</p>;
	}

	if (query.isError || !query.data) {
		return <p className="form-error">This role could not be loaded.</p>;
	}

	return (
		<PositionWizard
			positionId={positionId}
			knownProjectId={query.data.position.projectId}
			initialValues={toFormValues(query.data.position)}
			onDirtyChange={onDirtyChange}
			onCancel={onCancel}
			onSaved={onSaved}
		/>
	);
}

const PositionWizardDialog = forwardRef<PositionWizardCommand, PositionWizardDialogProps>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<Target | null>(null);

		const { setDirty, confirming, requestClose, discard, keepEditing } = useUnsavedChangesGuard(
			() => setTarget(null),
		);

		useImperativeHandle(
			ref,
			() => ({
				open: (projectId?: string) => {
					setDirty(false);
					setTarget({ mode: "open", projectId });
				},
				edit: (positionId: string) => {
					setDirty(false);
					setTarget({ mode: "edit", positionId });
				},
			}),
			[setDirty],
		);

		if (!target) {
			return null;
		}

		const editing = target.mode === "edit";

		const saved = () => {
			discard();
			onSuccess();
		};

		return (
			<>
				<Dialog
					open={true}
					title={editing ? "Edit position" : "New position"}
					maxWidth="wide"
					onClose={requestClose}
				>
					{editing ? (
						<EditContent
							key={target.positionId}
							positionId={target.positionId}
							onDirtyChange={setDirty}
							onCancel={requestClose}
							onSaved={saved}
						/>
					) : (
						<PositionWizard
							key={target.projectId ?? "open"}
							knownProjectId={target.projectId}
							initialValues={
								target.projectId ? { ...emptyPosition, projectId: target.projectId } : emptyPosition
							}
							onDirtyChange={setDirty}
							onCancel={requestClose}
							onSaved={saved}
						/>
					)}
				</Dialog>

				<ConfirmDialog
					open={confirming}
					title={editing ? "Discard these changes?" : "Discard this position?"}
					description="Nothing has been saved yet. Closing now throws away everything filled in so far."
					confirmLabel="Discard"
					onConfirm={discard}
					onClose={keepEditing}
				/>
			</>
		);
	},
);

PositionWizardDialog.displayName = "PositionWizardDialog";

export { PositionWizardDialog };
