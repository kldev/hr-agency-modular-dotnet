import { useForm } from "@tanstack/react-form";
import type { BadRequestDetails, CompanyContactRequest, ContactPerson } from "@/api/models";
import { Toggle } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";
import { ContactPersonForm } from "@/features/company-contacts/components/ContactPersonForm";

interface CompanyContactFormProps {
	initialValue: CompanyContactRequest;
	onSubmit: (value: CompanyContactRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
}

export function CompanyContactForm({
	initialValue,
	onSubmit,
	error,
	isSubmitting = false,
}: CompanyContactFormProps) {
	const form = useForm({
		defaultValues: initialValue,

		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	return (
		<form
			id="company-contact-form"
			className="drawer-form"
			onSubmit={(event) => {
				event.preventDefault();
				void form.handleSubmit();
			}}
		>
			<form.Field
				name="contact"
				validators={{
					onChange: ({ value }) => {
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
				{(field) => (
					<div className="form-field">
						<span className="form-label">Contact</span>

						<ContactPersonForm
							value={field.state.value as ContactPerson}
							mode="contact-form"
							disabled={isSubmitting}
							onChange={field.handleChange}
						/>

						{field.state.meta.errors.length > 0 && (
							<div className="form-field-error" role="alert">
								{field.state.meta.errors.map((error, index) => (
									<div key={index}>{String(error)}</div>
								))}
							</div>
						)}
					</div>
				)}
			</form.Field>

			<form.Field name="updatePrimary">
				{(field) => (
					<div className="form-field">
						<div className="flex items-center justify-between">
							<div>
								<label className="form-label" htmlFor="update-primary">
									Primary contact
								</label>

								<div className="form-help">Set this contact as the company's primary contact.</div>
							</div>

							<Toggle
								id="update-primary"
								checked={field.state.value ?? false}
								disabled={isSubmitting}
								onChange={(event) => field.handleChange(event.target.checked)}
							/>
						</div>
					</div>
				)}
			</form.Field>

			<ApiError error={error as unknown as BadRequestDetails} />
		</form>
	);
}
