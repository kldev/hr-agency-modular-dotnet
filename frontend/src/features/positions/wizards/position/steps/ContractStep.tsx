import { FileSignature } from "lucide-react";
import type { CurrencyCode, RateBasis, RateUnit, WorkerContractType } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import { currenciesOptions } from "@/features/sales/types";
import { rateBases, rateUnits, workerContractTypes } from "../../../types";
import { emptyPosition } from "../schema";

/**
 * What we sign with the person. Not the same question as the engagement type, which says how we
 * serve the client — the same role can hold somebody on an employment contract next to somebody on
 * a mandate, under one engagement.
 */
export const ContractStep = withForm({
	defaultValues: emptyPosition,

	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={FileSignature}
					title="Contract"
					description="What this role is paid and on what paper. The rate here is the role's proposal - what somebody actually gets belongs to their own contract."
				/>

				<form.AppField name="contractType">
					{(field) => (
						<field.FormChoiceGroup<WorkerContractType>
							label="Contract type"
							options={workerContractTypes}
							columns={2}
							fieldName={field.name}
							fieldValue={field.state.value as WorkerContractType}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<div className="form-wizard__grid">
					<form.AppField name="rateAmount">
						{(field) => (
							<field.FormMoneyInput
								label="Proposed rate"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>

					<form.AppField name="rateCurrency">
						{(field) => (
							<field.FormSelectEnum<CurrencyCode>
								label="Currency"
								options={currenciesOptions}
								fieldName={field.name}
								fieldValue={field.state.value as CurrencyCode}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
							/>
						)}
					</form.AppField>

					<form.AppField name="rateUnit">
						{(field) => (
							<field.FormSelectEnum<RateUnit>
								label="Per"
								options={rateUnits}
								fieldName={field.name}
								fieldValue={field.state.value as RateUnit}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
							/>
						)}
					</form.AppField>

					{/* Without gross or net a rate is a number two people read differently. */}
					<form.AppField name="rateBasis">
						{(field) => (
							<field.FormSelectEnum<RateBasis>
								label="Basis"
								options={rateBases}
								fieldName={field.name}
								fieldValue={field.state.value as RateBasis}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
							/>
						)}
					</form.AppField>

					<form.AppField name="payoutDay">
						{(field) => (
							<field.FormInput
								label="Paid by day of month"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>

					<form.AppField name="probationPeriod">
						{(field) => (
							<field.FormInput
								label="Probation period"
								placeholder="e.g. 1 month"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>

					<form.AppField name="noticePeriod">
						{(field) => (
							<field.FormInput
								label="Notice period"
								placeholder="e.g. 2 weeks"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>
				</div>

				<form.AppField name="allowances">
					{(field) => (
						<field.FormArrayField
							label="Allowances"
							placeholder="e.g. Accommodation provided"
							fieldName={field.name}
							fieldValue={field.state.value}
							values={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							onChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<div className="form-hint">
					On a posting the allowances are half the offer, so they are named rather than folded into
					the rate: accommodation, transport, per diem.
				</div>
			</FormWizard.Section>
		);
	},
});
