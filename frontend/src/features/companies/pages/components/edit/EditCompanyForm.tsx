import { useForm } from "@tanstack/react-form";
import type { BadRequestDetails, Industry, UpdateCompanyRequest } from "@/api/models";
import { CountrySelect, EnumSelectFilter, FieldError, Input } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";
import { industries } from "@/features/companies/types";

interface EditCompanyFormProps {
	initialValue: UpdateCompanyRequest;
	taxId: string;
	onSubmit: (company: UpdateCompanyRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
}

export function EditCompanyForm({
	initialValue,
	taxId,
	onSubmit,
	error,
	isSubmitting = false,
}: EditCompanyFormProps) {
	const form = useForm({
		defaultValues: initialValue,

		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	return (
		<form
			id="edit-company-form"
			className="drawer-form"
			onSubmit={(event) => {
				event.preventDefault();
				void form.handleSubmit();
			}}
		>
			<div className="form-field">
				<label className="form-label" htmlFor="company-tax-id">
					VAT / Tax ID
				</label>

				<Input id="company-tax-id" value={taxId} disabled readOnly />
			</div>

			<form.Field
				name="name"
				validators={{
					onChange: ({ value }) => {
						if (!value.trim()) {
							return "Company name is required";
						}

						if (value.trim().length < 2) {
							return "Company name must be at least 2 characters";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Company name
						</label>

						<Input
							id={field.name}
							name={field.name}
							value={field.state.value}
							disabled={isSubmitting}
							onBlur={field.handleBlur}
							onChange={(event) => field.handleChange(event.target.value)}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field name="countryCode">
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Country
						</label>

						<CountrySelect
							value={field.state.value}
							disabled={isSubmitting}
							onChange={(event) => field.handleChange(event.target.value)}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field name="industry">
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Industry
						</label>

						<EnumSelectFilter
							hideAll
							value={field.state.value}
							options={industries}
							onChange={(value) => field.handleChange(value ?? ("Other" as Industry))}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field
				name="webSite"
				validators={{
					onChange: ({ value }) => {
						if (!value.trim()) {
							return undefined;
						}

						try {
							new URL(value);
							return undefined;
						} catch {
							return "Enter a valid website URL";
						}
					},
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Website
						</label>

						<Input
							id={field.name}
							name={field.name}
							value={field.state.value}
							disabled={isSubmitting}
							onBlur={field.handleBlur}
							onChange={(event) => field.handleChange(event.target.value)}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field
				name="registrationNumber"
				validators={{
					onChange: ({ value }) => {
						if (!value.trim()) {
							return "Registration number is required";
						}

						if (value.trim().length < 3) {
							return "Registration number must be at least 3 characters";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Registration number
						</label>

						<Input
							id={field.name}
							name={field.name}
							value={field.state.value}
							disabled={isSubmitting}
							onBlur={field.handleBlur}
							onChange={(event) => field.handleChange(event.target.value)}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<ApiError error={error as unknown as BadRequestDetails} />
		</form>
	);
}
