import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import type { WorkerContractType } from "@/api/models";
import { useStartAgencyEmployment } from "../pages/hooks";
import { contractRequiresTimeRecord, maxWeeklyHours, workerContractTypes } from "../types";
import type { StartEmploymentFormCommand } from "./EmploymentFormCommand";

interface Props {
	onSuccess: () => void;
}

const schema = z.object({
	userId: z.string().min(1, "Pick the person"),
	contractType: z.string().min(1, "Pick what they work on"),
	startsOn: z.string().min(1, "Say when it begins"),
	weeklyHours: z
		.string()
		.refine(
			(value) => value === "" || (Number(value) >= 0 && Number(value) <= maxWeeklyHours),
			`Weekly hours must be between 0 and ${maxWeeklyHours}.`,
		),
});

const FormContent: React.FC<{
	userId: string;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ userId, onSuccess, handleClose }) => {
	const { mutation, waiting } = useStartAgencyEmployment({
		onSuccess: () => {
			mutation.reset();
			toast.success("Employment recorded");
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: {
			userId,
			contractType: "",
			startsOn: "",
			weeklyHours: "",
		},

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				request: {
					userId: value.userId,
					contractType: value.contractType as WorkerContractType,
					startsOn: value.startsOn.slice(0, 10),
					weeklyHours: value.weeklyHours === "" ? null : Number(value.weeklyHours),
				},
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Record employment"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						{userId ? null : (
							<form.AppField name="userId">
								{(field) => (
									<field.FormUserPicker
										label="Person"
										placeholder="Search people ..."
										fieldValue={{ id: field.state.value }}
										errors={field.state.meta.errors}
										fieldName={field.name}
										handleChange={(value) => field.handleChange(value.id ?? "")}
										isSubmitting={mutation.isPending}
									/>
								)}
							</form.AppField>
						)}

						<form.AppField name="contractType">
							{(field) => (
								<field.FormSelectEnum
									label="Contract"
									options={workerContractTypes}
									fieldValue={field.state.value as WorkerContractType}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						{/*
						 * The consequence of the choice, said before it is saved: this is the whole
						 * reason the register exists, and the monitoring screen is built on it.
						 */}
						<form.Subscribe selector={(state) => state.values.contractType}>
							{(contractType) =>
								contractType ? (
									<div className="form-hint">
										{contractRequiresTimeRecord[contractType as WorkerContractType]
											? "This contract carries the duty to record hours - they will appear on the time sheet monitoring."
											: "This contract carries no duty to record hours - no time sheet is expected."}
									</div>
								) : null
							}
						</form.Subscribe>

						<form.AppField name="startsOn">
							{(field) => (
								<field.FormDatePicker
									label="Starts on"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="weeklyHours">
							{(field) => (
								<field.FormInput
									label="Weekly hours"
									type="number"
									min={0}
									max={maxWeeklyHours}
									step={0.5}
									placeholder="Optional"
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

const StartEmploymentDrawer = forwardRef<StartEmploymentFormCommand, Props>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<{ userId: string } | null>(null);

		useImperativeHandle(
			ref,
			() => ({ start: (userId) => setTarget({ userId: userId ?? "" }) }),
			[],
		);

		if (!target) return null;

		return (
			<FormContent
				userId={target.userId}
				onSuccess={onSuccess}
				handleClose={() => setTarget(null)}
			/>
		);
	},
);

StartEmploymentDrawer.displayName = "StartEmploymentDrawer";

export default StartEmploymentDrawer;
