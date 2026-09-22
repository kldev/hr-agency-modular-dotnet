import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import type {
	AssignmentProjection,
	ComplianceRequirementView,
	ComplianceStatus,
} from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import {
	complianceRequirementHints,
	complianceRequirements,
	complianceStatuses,
} from "@/features/compliance";
import { toDateOnly } from "@/utlis/formatRecord";
import { useRecordAssignmentComplianceItem } from "../pages/hooks";
import type { RecordAssignmentComplianceFormCommand } from "./AssignmentFormCommand";

interface RecordAssignmentComplianceDrawerProps {
	onSuccess: () => void;
}

type ComplianceTarget = {
	assignment: AssignmentProjection;
	view: ComplianceRequirementView;
};

const NO_DOCUMENT = "";

/*
 * The backend rule, checked here as well: confirming a numbered instrument without writing the
 * number down records that somebody remembers doing it, which is not the same as being able to
 * prove it. A disabled button beats a 400 for something the form already knows.
 */
function complianceSchema(requiresReferenceNumber: boolean) {
	return z
		.object({
			status: z.string().min(1, "Pick a status"),
			referenceNumber: z.string().trim(),
			validFrom: z.string(),
			validTo: z.string(),
			documentId: z.string(),
			note: z.string().trim().max(500, "The note cannot exceed 500 characters."),
		})
		.superRefine((value, context) => {
			if (requiresReferenceNumber && value.status === "Confirmed" && !value.referenceNumber) {
				context.addIssue({
					code: "custom",
					path: ["referenceNumber"],
					message: "This one is confirmed by its number. Record it to confirm the item.",
				});
			}

			if (value.validFrom && value.validTo && value.validTo < value.validFrom) {
				context.addIssue({
					code: "custom",
					path: ["validTo"],
					message: "The end of validity cannot precede its start.",
				});
			}
		});
}

const FormContent: React.FC<{
	target: ComplianceTarget;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ target, onSuccess, handleClose }) => {
	const item = target.view.item;

	const { mutation, waiting } = useRecordAssignmentComplianceItem({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	/*
	 * Only this posting's documents. An A1 proven by a file hanging off the project would be proof
	 * about everybody at once, which is exactly the tick this register replaced.
	 */
	const documentOptions: Record<string, string> = {
		[NO_DOCUMENT]: "No document",
		...Object.fromEntries(
			target.assignment.documents.map((document) => [document.documentId, document.fileName]),
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

		validators: { onChange: complianceSchema(target.view.requiresReferenceNumber) },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				assignmentId: target.assignment.id,
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

						{/* Which posting this is about: one named person, one period, one posting company -
						    which is how these instruments are issued in the first place. */}
						<div className="form-hint">
							{target.assignment.workerFullName}, {target.assignment.projectName} (
							{target.assignment.workCountry}), posted by {target.assignment.deliveringEntityName}.
						</div>

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
											? "Reference number (required to confirm this item)"
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

						{target.assignment.documents.length === 0 ? (
							<div className="form-hint">
								Attach the document to this assignment first and it will show up here as proof.
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

const RecordAssignmentComplianceDrawer = forwardRef<
	RecordAssignmentComplianceFormCommand,
	RecordAssignmentComplianceDrawerProps
>(({ onSuccess }, ref) => {
	const [target, setTarget] = useState<ComplianceTarget | null>(null);

	useImperativeHandle(
		ref,
		() => ({
			record: (assignment: AssignmentProjection, view: ComplianceRequirementView) => {
				setTarget({ assignment, view });
			},
		}),
		[],
	);

	if (!target) return null;

	return <FormContent target={target} onSuccess={onSuccess} handleClose={() => setTarget(null)} />;
});

RecordAssignmentComplianceDrawer.displayName = "RecordAssignmentComplianceDrawer";

export { RecordAssignmentComplianceDrawer };
