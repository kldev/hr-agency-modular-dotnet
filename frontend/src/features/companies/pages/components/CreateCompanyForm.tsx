import { useForm } from "@tanstack/react-form";
import { type BadRequestDetails, type CreateCompanyRequest, Industry } from "@/api/models";
import { CountrySelect, EnumSelectFilter, Input } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";
import { industries } from "../../types";

interface CompanyFormProps {
	initialValue?: CreateCompanyRequest;
	onSubmit: (company: CreateCompanyRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
}

export const emptyForm: CreateCompanyRequest = {
	name: "",
	website: "",
	registrationNumber: "",
	taxId: "",
	countryCode: "PL",
	industry: "Technology",
};

function FieldError({ errors }: { errors: Array<unknown> }) {
	if (errors.length === 0) {
		return null;
	}

	return (
		<div className="form-field-error" role="alert">
			{errors.map((error, index) => (
				<div key={index}>{String(error)}</div>
			))}
		</div>
	);
}

export function CreateCompanyForm({
	initialValue,
	onSubmit,
	error,
	isSubmitting = false,
}: CompanyFormProps) {
	const form = useForm({
		defaultValues: initialValue ?? emptyForm,

		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	return (
		<form
			id="company-form"
			className="drawer-form"
			onSubmit={(event) => {
				event.preventDefault();
				void form.handleSubmit();
			}}
		>
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

			<form.Field
				name="taxId"
				validators={{
					onChange: ({ value }) => {
						if (!value.trim()) {
							return "Tax ID is required";
						}

						if (value.trim().length < 3) {
							return "Tax ID must be at least 3 characters";
						}

						return undefined;
					},

					// onChangeAsync: async ({ value }) => {
					// 	if (!value.trim()) {
					// 		return undefined;
					// 	}

					// 	// await checkTaxId(value);

					// 	return undefined;
					// },

					// onChangeAsyncDebounceMs: 500,
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							VAT / Tax ID
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
							onChange={(value) => field.handleChange(value ?? Industry.Other)}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field
				name="website"
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
						return undefined
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
