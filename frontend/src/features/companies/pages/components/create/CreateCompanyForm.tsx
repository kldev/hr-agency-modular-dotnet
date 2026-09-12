import { useForm } from "@tanstack/react-form";
import { useState } from "react";
import {
	type BadRequestDetails,
	type ContactPerson,
	type CreateCompanyRequest,
	Industry,
} from "@/api/models";
import { CountrySelect, EnumSelectFilter, FieldError, Input, Toggle } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";
import { industries } from "@/features/companies/types";
import { ContactPersonForm } from "@/features/company-contacts/components/ContactPersonForm";

interface CompanyFormProps {
	initialValue?: CreateCompanyRequest;
	onSubmit: (company: CreateCompanyRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
}

const emptyContact: ContactPerson = {
	email: "",
	firstName: "",
	jobTitle: "",
	lastName: "",
	phone: "",
};
export const emptyForm: CreateCompanyRequest = {
	name: "",
	website: "",
	registrationNumber: "",
	taxId: "",
	countryCode: "PL",
	industry: "Technology",
	contact: emptyContact,
};

export function CreateCompanyForm({
	initialValue,
	onSubmit,
	error,
	isSubmitting = false,
}: CompanyFormProps) {
	const form = useForm({
		defaultValues: initialValue ?? emptyForm,

		onSubmit: async ({ value }) => {
			if (hasContact) {
				onSubmit(value);
			} else {
				onSubmit({ ...value, contact: undefined });
			}
		},
	});

	const [hasContact, setHasContact] = useState(true);

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
			<form.Field
				name="contact"
				validators={{
					onChange: ({ value }) => {
						if (!value || hasContact === false) {
							return undefined;
						}

						if (!value.firstName.trim()) {
							return "First name is required";
						}

						if (!value.lastName.trim()) {
							return "Last name is required";
						}

						if (!value.email.trim()) {
							return "Email is required";
						}

						if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.email)) {
							return "Enter a valid email address";
						}

						return undefined;
					},
				}}
			>
				{(field) => {
					return (
						<div className="form-field">
							<div className="flex items-center justify-between">
								<label className="form-label" htmlFor="company-contact-toggle">
									Contact
								</label>

								<Toggle
									id="company-contact-toggle"
									checked={hasContact}
									disabled={isSubmitting}
									onChange={(event) => {
										setHasContact(event.target.checked);
									}}
								/>
							</div>

							{hasContact && (
								<ContactPersonForm
									value={field.state.value as ContactPerson}
									mode="company-create-form"
									disabled={isSubmitting}
									onChange={field.handleChange}
								/>
							)}
							<FieldError errors={field.state.meta.errors} />
						</div>
					);
				}}
			</form.Field>

			<ApiError error={error as unknown as BadRequestDetails} />
		</form>
	);
}
