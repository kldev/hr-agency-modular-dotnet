import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useAuthStore } from "#/stores/authStore";
import type { AgencyEmploymentProjection, WorkerContractType } from "@/api/models";
import { useChangeAgencyEmploymentTerms } from "../pages/hooks";
import { contractRequiresTimeRecord, isRates, maxWeeklyHours, workerContractTypes } from "../types";
import type { ChangeEmploymentTermsFormCommand } from "./EmploymentFormCommand";
import {
	RateFields,
	rateFieldNames,
	rateFieldsOf,
	rateFieldsSchema,
	toRateInput,
} from "./RateFields";

interface Props {
	onSuccess: () => void;
}

const schema = rateFieldsSchema.extend({
	contractType: z.string().min(1, "Pick what they work on"),
	effectiveFrom: z.string().min(1, "Say when the new terms start"),
	weeklyHours: z
		.string()
		.refine(
			(value) => value === "" || (Number(value) >= 0 && Number(value) <= maxWeeklyHours),
			`Weekly hours must be between 0 and ${maxWeeklyHours}.`,
		),
});

const FormContent: React.FC<{
	employment: AgencyEmploymentProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ employment, onSuccess, handleClose }) => {
	const { mutation, waiting } = useChangeAgencyEmploymentTerms({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const mayQuoteRate = isRates(useAuthStore((state) => state.user?.role));

	const form = useAppForm({
		defaultValues: {
			contractType: employment.contractType as string,
			effectiveFrom: "",
			weeklyHours: employment.weeklyHours === null ? "" : String(employment.weeklyHours),
			...rateFieldsOf(employment.rate),
		},

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				userId: employment.userId,
				request: {
					contractType: value.contractType as WorkerContractType,
					effectiveFrom: value.effectiveFrom.slice(0, 10),
					weeklyHours: value.weeklyHours === "" ? null : Number(value.weeklyHours),
					// The terms are replaced whole, and clearing the amount removes the rate. Somebody
					// not shown pay sends none, and the backend keeps the one in force.
					rate: mayQuoteRate ? toRateInput(value) : null,
				},
			});
		},
	});

	const person = `${employment.user.firstName} ${employment.user.lastName}`;

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`Terms for ${person}`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
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

						{/* Moving somebody off a mandate takes them off the monitoring list, and back on. */}
						<form.Subscribe selector={(state) => state.values.contractType}>
							{(contractType) =>
								contractType &&
								contractRequiresTimeRecord[contractType as WorkerContractType] !==
									contractRequiresTimeRecord[employment.contractType] ? (
									<div className="form-hint">
										{contractRequiresTimeRecord[contractType as WorkerContractType]
											? "From now on this person owes hours and will appear on the time sheet monitoring."
											: "This person will no longer owe hours. Months already recorded stay as they are."}
									</div>
								) : null
							}
						</form.Subscribe>

						<form.AppField name="effectiveFrom">
							{(field) => (
								<field.FormDatePicker
									label="Effective from"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						{/* Mirrors `EffectiveBeforeStartMessage`, said where the date is picked. */}
						<div className="form-hint">
							The engagement began on {employment.startsOn.slice(0, 10)}; new terms cannot take
							effect before that.
						</div>

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

						{mayQuoteRate ? (
							<RateFields form={form} fields={rateFieldNames} isSubmitting={mutation.isPending} />
						) : null}

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

const ChangeEmploymentTermsDrawer = forwardRef<ChangeEmploymentTermsFormCommand, Props>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<AgencyEmploymentProjection | null>(null);

		useImperativeHandle(ref, () => ({ changeTerms: setTarget }), []);

		if (!target) return null;

		return (
			<FormContent employment={target} onSuccess={onSuccess} handleClose={() => setTarget(null)} />
		);
	},
);

ChangeEmploymentTermsDrawer.displayName = "ChangeEmploymentTermsDrawer";

export default ChangeEmploymentTermsDrawer;
