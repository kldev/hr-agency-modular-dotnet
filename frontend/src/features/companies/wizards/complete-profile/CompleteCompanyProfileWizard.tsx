import { Banknote, Building2, MapPin, UserCheck } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";
import type { BadRequestDetails, CompanyProjection } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { DirtyReporter, stepHasErrors } from "#/components/form-wizard/stepValidation";
import { ApiError } from "#/components/ui/ApiError";
import { useAppForm } from "#/forms";
import { type CompanyProfileFormValues, companyProfileSchema, toFormValues } from "./schema";
import { companyProfileSteps } from "./steps";
import { useCompleteCompanyProfile } from "./useCompleteCompanyProfile";

interface CompleteCompanyProfileWizardProps {
	company: CompanyProjection;
	onSaved: () => void;
	onCancel: () => void;
	onDirtyChange: (dirty: boolean) => void;
}

export function CompleteCompanyProfileWizard({
	company,
	onSaved,
	onCancel,
	onDirtyChange,
}: CompleteCompanyProfileWizardProps) {
	const [currentStep, setCurrentStep] = useState(0);

	const { mutation } = useCompleteCompanyProfile({
		onSuccess: () => {
			onSaved();
		},
	});

	const form = useAppForm({
		// Seeded once. Re-seeding from a refetch would either drop what the user has typed or throw
		// away the fact that they touched anything.
		defaultValues: toFormValues(company),

		validators: {
			onChange: companyProfileSchema,
		},

		onSubmit: async ({ value }) => {
			const hasRepresentative = value.representativeFirstName !== "";

			try {
				await mutation.mutateAsync({
					companyId: company.id,
					request: {
						legalName: value.legalName || null,
						street: value.street || null,
						buildingNumber: value.buildingNumber || null,
						unitNumber: value.unitNumber || null,
						postalCode: value.postalCode || null,
						city: value.city || null,
						countryCode: value.countryCode ? value.countryCode.toUpperCase() : null,
						vatNumber: value.vatNumber || null,
						iban: value.iban || null,
						bic: value.bic || null,
						legalRepresentative: hasRepresentative
							? {
									firstName: value.representativeFirstName,
									lastName: value.representativeLastName,
									jobTitle: value.representativeJobTitle,
									phone: value.representativePhone,
									email: value.representativeEmail,
								}
							: null,
					},
				});
			} catch (error) {
				const details = error as BadRequestDetails;

				toast.error(details?.title ?? "Unable to save the client data");
			}
		},
	});

	const handleNext = async () => {
		const fields = companyProfileSteps[currentStep].fields;

		for (const field of fields) {
			await form.validateField(field, "submit");
		}

		const fieldMeta = form.state.fieldMeta as Record<string, { errors: Array<unknown> }>;

		if (stepHasErrors(fieldMeta, fields)) {
			return;
		}

		setCurrentStep((step) => Math.min(step + 1, companyProfileSteps.length - 1));
	};

	return (
		<FormWizard className="form-wizard--in-dialog">
			<form.Subscribe selector={(state) => state.isDirty}>
				{(isDirty) => <DirtyReporter isDirty={isDirty} onDirtyChange={onDirtyChange} />}
			</form.Subscribe>

			<FormWizard.Header steps={companyProfileSteps} currentStep={currentStep} />

			<FormWizard.Body>
				<FormWizard.Content>
					{currentStep === 0 && (
						<FormWizard.Section>
							<FormWizard.SectionHeader
								icon={Building2}
								title="Legal identity"
								description="The name a contract states, not the one the sales pipeline uses."
							/>

							<FormWizard.Field label="Tax ID">
								<p className="form-wizard__readonly-value">{company.taxId || "—"}</p>
							</FormWizard.Field>

							<FormWizard.Field label="Registration number">
								<p className="form-wizard__readonly-value">{company.registrationNumber || "—"}</p>
							</FormWizard.Field>

							<form.AppField name="legalName">
								{(field) => (
									<field.FormInput
										label="Legal name"
										fieldName={field.name}
										fieldValue={field.state.value}
										errors={field.state.meta.errors}
										handleChange={(value) => field.handleChange(value)}
										isSubmitting={mutation.isPending}
									/>
								)}
							</form.AppField>

							<form.AppField name="vatNumber">
								{(field) => (
									<field.FormInput
										label="EU VAT number"
										fieldName={field.name}
										fieldValue={field.state.value}
										errors={field.state.meta.errors}
										handleChange={(value) => field.handleChange(value)}
										isSubmitting={mutation.isPending}
									/>
								)}
							</form.AppField>
						</FormWizard.Section>
					)}

					{currentStep === 1 && (
						<FormWizard.Section>
							<FormWizard.SectionHeader
								icon={MapPin}
								title="Registered address"
								description="Where the company is registered. Together with the legal name, this is what makes the profile complete."
							/>

							<div className="form-wizard__grid">
								<form.AppField name="street">
									{(field) => (
										<field.FormInput
											label="Street"
											fieldName={field.name}
											fieldValue={field.state.value}
											errors={field.state.meta.errors}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>

								<form.AppField name="buildingNumber">
									{(field) => (
										<field.FormInput
											label="Building number"
											fieldName={field.name}
											fieldValue={field.state.value}
											errors={field.state.meta.errors}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>

								<form.AppField name="unitNumber">
									{(field) => (
										<field.FormInput
											label="Unit number"
											fieldName={field.name}
											fieldValue={field.state.value}
											errors={field.state.meta.errors}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>

								<form.AppField name="postalCode">
									{(field) => (
										<field.FormInput
											label="Postal code"
											fieldName={field.name}
											fieldValue={field.state.value}
											errors={field.state.meta.errors}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>

								<form.AppField name="city">
									{(field) => (
										<field.FormInput
											label="City"
											fieldName={field.name}
											fieldValue={field.state.value}
											errors={field.state.meta.errors}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>

								<form.AppField name="countryCode">
									{(field) => (
										<field.FormCountrySelect
											label="Country"
											fieldName={field.name}
											fieldValue={field.state.value}
											errors={field.state.meta.errors}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>
							</div>
						</FormWizard.Section>
					)}

					{currentStep === 2 && (
						<FormWizard.Section>
							<FormWizard.SectionHeader
								icon={Banknote}
								title="Billing"
								description="Optional, and needed as soon as anybody wants to be paid."
							/>

							<form.AppField name="iban">
								{(field) => (
									<field.FormInput
										label="IBAN"
										fieldName={field.name}
										fieldValue={field.state.value}
										errors={field.state.meta.errors}
										handleChange={(value) => field.handleChange(value)}
										isSubmitting={mutation.isPending}
									/>
								)}
							</form.AppField>

							<form.AppField name="bic">
								{(field) => (
									<field.FormInput
										label="BIC"
										fieldName={field.name}
										fieldValue={field.state.value}
										errors={field.state.meta.errors}
										handleChange={(value) => field.handleChange(value)}
										isSubmitting={mutation.isPending}
									/>
								)}
							</form.AppField>
						</FormWizard.Section>
					)}

					{currentStep === 3 && (
						<FormWizard.Section>
							<FormWizard.SectionHeader
								icon={UserCheck}
								title="Representation"
								description="The person who signs for the company. Optional here, and the contract will ask for somebody."
							/>

							<div className="form-wizard__grid">
								<form.AppField name="representativeFirstName">
									{(field) => (
										<field.FormInput
											label="First name"
											fieldName={field.name}
											fieldValue={field.state.value}
											errors={field.state.meta.errors}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>

								<form.AppField name="representativeLastName">
									{(field) => (
										<field.FormInput
											label="Last name"
											fieldName={field.name}
											fieldValue={field.state.value}
											errors={field.state.meta.errors}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>

								<form.AppField name="representativeJobTitle">
									{(field) => (
										<field.FormInput
											label="Job title"
											fieldName={field.name}
											fieldValue={field.state.value}
											errors={field.state.meta.errors}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>

								<form.AppField name="representativePhone">
									{(field) => (
										<field.FormInput
											label="Phone"
											fieldName={field.name}
											fieldValue={field.state.value}
											errors={field.state.meta.errors}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>

								<form.AppField name="representativeEmail">
									{(field) => (
										<field.FormInput
											label="E-mail"
											fieldName={field.name}
											fieldValue={field.state.value}
											errors={field.state.meta.errors}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>
							</div>
						</FormWizard.Section>
					)}

					{currentStep === 4 && <ReviewStep values={form.state.values} taxId={company.taxId} />}
				</FormWizard.Content>

				<ApiError error={(mutation.error as unknown as BadRequestDetails) ?? null} />

				<form.Subscribe selector={(state) => state.canSubmit}>
					{(canSubmit) => (
						<FormWizard.Footer
							currentStep={currentStep}
							stepCount={companyProfileSteps.length}
							onBack={() => setCurrentStep((step) => Math.max(step - 1, 0))}
							onNext={handleNext}
							onSubmit={() => {
								void form.handleSubmit();
							}}
							onCancel={onCancel}
							canSubmit={canSubmit}
							isSubmitting={mutation.isPending}
							submitLabel="Save client data"
						/>
					)}
				</form.Subscribe>
			</FormWizard.Body>
		</FormWizard>
	);
}

function ReviewStep({ values, taxId }: { values: CompanyProfileFormValues; taxId: string }) {
	const missing: string[] = [];

	if (!values.legalName) missing.push("the legal name");
	if (!values.street || !values.city || !values.countryCode) missing.push("the registered address");
	if (!taxId) missing.push("the tax ID (edit the company to add it)");

	return (
		<FormWizard.Section>
			<FormWizard.SectionHeader
				title="Review"
				description="Everything here is optional on its own. Two of them decide whether the client counts as complete."
			/>

			<div className="form-wizard__summary">
				<div className="form-wizard__summary-section">
					<h3 className="form-wizard__summary-title">Legal identity</h3>

					<div className="form-wizard__summary-grid">
						<SummaryItem label="Legal name" value={values.legalName} />
						<SummaryItem label="Tax ID" value={taxId} />
						<SummaryItem label="EU VAT number" value={values.vatNumber} />
					</div>
				</div>

				<div className="form-wizard__summary-section">
					<h3 className="form-wizard__summary-title">Registered address</h3>

					<div className="form-wizard__summary-grid">
						<SummaryItem
							label="Address"
							value={
								values.street
									? `${values.street} ${values.buildingNumber}${
											values.unitNumber ? `/${values.unitNumber}` : ""
										}, ${values.postalCode} ${values.city}, ${values.countryCode.toUpperCase()}`
									: ""
							}
						/>
					</div>
				</div>

				<div className="form-wizard__summary-section">
					<h3 className="form-wizard__summary-title">Billing and representation</h3>

					<div className="form-wizard__summary-grid">
						<SummaryItem label="IBAN" value={values.iban} />
						<SummaryItem label="BIC" value={values.bic} />
						<SummaryItem
							label="Representative"
							value={
								values.representativeFirstName
									? `${values.representativeFirstName} ${values.representativeLastName}, ${values.representativeJobTitle}`
									: ""
							}
						/>
					</div>
				</div>

				<div className="form-wizard__summary-section">
					<h3 className="form-wizard__summary-title">Completeness</h3>

					<p className="form-wizard__section-description">
						{missing.length === 0
							? "Everything a contract needs is here. Projects for this client can go live."
							: `Still missing: ${missing.join(", ")}. The data can be saved as it is - a project just cannot go live until this is filled in.`}
					</p>
				</div>
			</div>
		</FormWizard.Section>
	);
}

function SummaryItem({ label, value }: { label: string; value: string }) {
	return (
		<div className="form-wizard__summary-item">
			<span className="form-wizard__summary-label">{label}</span>

			<span className="form-wizard__summary-value">{value || "—"}</span>
		</div>
	);
}
