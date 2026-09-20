import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import type { ContractStatus, ProjectProjection } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useRecordProjectContract } from "../pages/hooks";
import { contractStatuses } from "../types";
import { toDateOnly } from "../utils";
import type { RecordContractFormCommand } from "./ProjectFormCommand";

interface RecordContractDrawerProps {
	onSuccess: () => void;
}

const contractSchema = z
	.object({
		contractNumber: z.string().trim().min(1, "Contract number is required"),
		status: z.string().min(1, "Pick a status"),
		signedOn: z.string(),
		validFrom: z.string().min(1, "Valid from is required"),
		validTo: z.string(),
	})
	.superRefine((value, context) => {
		if (value.status === "Signed" && !value.signedOn) {
			context.addIssue({
				code: "custom",
				path: ["signedOn"],
				message: "A signed contract needs the date it was signed.",
			});
		}

		if (value.validTo && value.validTo < value.validFrom) {
			context.addIssue({
				code: "custom",
				path: ["validTo"],
				message: "The end date cannot be earlier than the start date.",
			});
		}
	});

const FormContent: React.FC<{
	project: ProjectProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ project, onSuccess, handleClose }) => {
	const contract = project.contract;

	const { mutation, waiting } = useRecordProjectContract({
		onSuccess: () => {
			mutation.reset();
			toast.success("Contract recorded");
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: {
			contractNumber: contract?.contractNumber ?? "",
			status: (contract?.status ?? "Draft") as string,
			signedOn: contract?.signedOn ?? "",
			validFrom: contract?.validFrom ?? project.startsOn,
			validTo: contract?.validTo ?? project.endsOn ?? "",
		},

		validators: {
			onChange: contractSchema,
		},

		onSubmit: async ({ value }) => {
			mutation.mutate({
				projectId: project.id,
				request: {
					contractNumber: value.contractNumber.trim(),
					status: value.status as ContractStatus,
					signedOn: value.signedOn ? toDateOnly(value.signedOn) : null,
					validFrom: toDateOnly(value.validFrom),
					validTo: value.validTo ? toDateOnly(value.validTo) : null,
				},
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={contract ? "Update contract" : "Record contract"}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField name="contractNumber">
							{(field) => (
								<field.FormInput
									label="Contract number"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="status">
							{(field) => (
								<field.FormSelectEnum
									label="Status"
									options={contractStatuses}
									fieldValue={field.state.value as ContractStatus}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="signedOn">
							{(field) => (
								<field.FormDatePicker
									label="Signed on"
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

						<div className="form-hint">
							The other party is taken from the client's profile as it stands now and frozen on the
							contract - later edits to the company will not change what this contract says. Who
							signed it comes from the contract signatory contact.
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

const RecordContractDrawer = forwardRef<RecordContractFormCommand, RecordContractDrawerProps>(
	({ onSuccess }, ref) => {
		const [project, setProject] = useState<ProjectProjection | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				record: (target: ProjectProjection) => {
					setProject(target);
				},
			}),
			[],
		);

		if (!project) return null;

		return (
			<FormContent project={project} onSuccess={onSuccess} handleClose={() => setProject(null)} />
		);
	},
);

RecordContractDrawer.displayName = "RecordContractDrawer";

export default RecordContractDrawer;
