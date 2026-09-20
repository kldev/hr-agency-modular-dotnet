import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import type { ComplianceRequirementView, ComplianceStatus, ProjectProjection } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useRecordComplianceItem } from "../pages/hooks";
import { complianceRequirementHints, complianceRequirements, complianceStatuses } from "../types";
import { toDateOnly } from "../utils";
import type { RecordComplianceFormCommand } from "./ProjectFormCommand";

interface RecordComplianceDrawerProps {
	onSuccess: () => void;
}

type ComplianceTarget = {
	project: ProjectProjection;
	view: ComplianceRequirementView;
};

const NO_DOCUMENT = "";

const complianceSchema = z.object({
	status: z.string().min(1, "Pick a status"),
	referenceNumber: z.string().trim(),
	validFrom: z.string(),
	validTo: z.string(),
	documentId: z.string(),
	note: z.string().trim().max(500, "The note cannot exceed 500 characters."),
});

const FormContent: React.FC<{
	target: ComplianceTarget;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ target, onSuccess, handleClose }) => {
	const item = target.view.item;

	const { mutation, waiting } = useRecordComplianceItem({
		onSuccess: () => {
			mutation.reset();
			toast.success("Compliance item recorded");
			onSuccess();
			handleClose();
		},
	});

	const documentOptions: Record<string, string> = {
		[NO_DOCUMENT]: "No document",
		...Object.fromEntries(
			target.project.documents.map((document) => [document.documentId, document.fileName]),
		),
	};

	const form = useAppForm({
		defaultValues: {
			status: (item?.status ?? "InProgress") as string,
			referenceNumber: item?.referenceNumber ?? "",
			validFrom: item?.validFrom ?? "",
			validTo: item?.validTo ?? "",
			documentId: item?.documentId ?? NO_DOCUMENT,
			note: item?.note ?? "",
		},

		validators: {
			onChange: complianceSchema,
		},

		onSubmit: async ({ value }) => {
			mutation.mutate({
				projectId: target.project.id,
				requirement: target.view.requirement,
				request: {
					status: value.status as ComplianceStatus,
					referenceNumber: value.referenceNumber.trim() || null,
					validFrom: value.validFrom ? toDateOnly(value.validFrom) : null,
					validTo: value.validTo ? toDateOnly(value.validTo) : null,
					documentId: value.documentId || null,
					note: value.note.trim() || null,
				},
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={complianceRequirements[target.view.requirement]}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<div className="form-hint">{complianceRequirementHints[target.view.requirement]}</div>

						<form.AppField name="status">
							{(field) => (
								<field.FormSelectEnum
									label="Status"
									options={complianceStatuses}
									fieldValue={field.state.value as ComplianceStatus}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="referenceNumber">
							{(field) => (
								<field.FormInput
									label={
										target.view.requiresReferenceNumber
											? "Reference number (required for this item)"
											: "Reference number"
									}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="validFrom">
							{(field) => (
								<field.FormDatePicker
									label="Valid from"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="validTo">
							{(field) => (
								<field.FormDatePicker
									label="Valid to"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="documentId">
							{(field) => (
								<field.FormSelectEnum
									label="Proof"
									options={documentOptions}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						{target.project.documents.length === 0 ? (
							<div className="form-hint">
								Attach the document to the project first and it will show up here as proof.
							</div>
						) : null}

						<form.AppField name="note">
							{(field) => (
								<field.FormTextAreaInput
									label="Note"
									rows={3}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
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

const RecordComplianceDrawer = forwardRef<RecordComplianceFormCommand, RecordComplianceDrawerProps>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<ComplianceTarget | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				record: (project: ProjectProjection, view: ComplianceRequirementView) => {
					setTarget({ project, view });
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

RecordComplianceDrawer.displayName = "RecordComplianceDrawer";

export default RecordComplianceDrawer;
