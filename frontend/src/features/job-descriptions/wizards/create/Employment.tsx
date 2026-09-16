import { WalletCards } from "lucide-react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { currenciesOptions } from "#/features/sales/types";
import { withForm } from "#/forms";
import { employmentTypesOptions, workModeOptions } from "../../type";
import type { JobDescriptionFormValues } from "./schema";

export const EmploymentStep = withForm({
	defaultValues: {} as JobDescriptionFormValues,
	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },
	render: function Render({ form, isSubmitting }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					title="Job details"
					description="Configure where the role is based, how the candidate will work and the compensation range."
				/>

				<form.AppField name="location">
					{(field) => (
						<field.FormInput
							label="Location"
							placeholder="e.g. Opole"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
						/>
					)}
				</form.AppField>

				<div className="form-wizard__grid">
					<form.AppField name="countryCode">
						{(field) => (
							<field.FormCountrySelect
								label="Country"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>

					<form.AppField name="employmentType">
						{(field) => (
							<field.FormSelectEnum
								label="Employment type"
								fieldName={field.name}
								fieldValue={field.state.value}
								options={employmentTypesOptions}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>

					<form.AppField name="workMode">
						{(field) => (
							<field.FormSelectEnum
								label="Work mode"
								fieldName={field.name}
								fieldValue={field.state.value}
								options={workModeOptions}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>
				</div>

				<section className="rounded-md border border-(--color-border) bg-(--color-surface-subtle) p-4 sm:p-5">
					<div className="mb-4 flex items-center gap-2">
						<WalletCards size={16} className="text-(--color-primary)" />
						<h3 className="text-sm font-semibold">Salary range</h3>
					</div>

					<div className="grid gap-4 md:grid-cols-3">
						<FormWizard.Field label="Minimum" required>
							<form.AppField name="salaryMin">
								{(field) => (
									<field.FormMoneyInput
										label=""
										fieldName={field.name}
										fieldValue={Number.isNaN(field.state.value) ? "" : field.state.value.toString()}
										errors={field.state.meta.errors}
										handleChange={(value) => field.handleChange(value)}
										isSubmitting={isSubmitting}
									/>
								)}
							</form.AppField>
						</FormWizard.Field>
						<FormWizard.Field label="Maximum" required>
							<form.AppField name="salaryMax">
								{(field) => (
									<field.FormMoneyInput
										label=""
										fieldName={field.name}
										fieldValue={Number.isNaN(field.state.value) ? "" : field.state.value.toString()}
										errors={field.state.meta.errors}
										handleChange={(value) => field.handleChange(value)}
										isSubmitting={isSubmitting}
									/>
								)}
							</form.AppField>
						</FormWizard.Field>
						<FormWizard.Field label="Currency" required>
							<form.AppField name="currencyCode">
								{(field) => (
									<field.FormSelectEnum
										label=""
										fieldName={field.name}
										fieldValue={field.state.value}
										options={currenciesOptions}
										errors={field.state.meta.errors}
										handleChange={(value) => field.handleChange(value)}
										isSubmitting={isSubmitting}
									/>
								)}
							</form.AppField>
						</FormWizard.Field>
					</div>
				</section>
			</FormWizard.Section>
		);
	},
});
