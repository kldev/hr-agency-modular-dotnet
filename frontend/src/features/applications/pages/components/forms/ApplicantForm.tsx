import { useForm } from "@tanstack/react-form";
import type { UpdateApplicantRequest } from "@/api/models";
import { FieldError, Input } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";

interface ApplicantFormProps {
	initialValue: UpdateApplicantRequest;
	onSubmit: (value: UpdateApplicantRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
}

export function ApplicantForm({
	initialValue,
	onSubmit,
	error,
	isSubmitting = false,
}: ApplicantFormProps) {
	const form = useForm({
		defaultValues: initialValue,

		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	return (
		<form
			id="applicant-form"
			className="drawer-form"
			onSubmit={(event) => {
				event.preventDefault();
				void form.handleSubmit();
			}}
		>
			<form.Field
				name="firstName"
				validators={{
					onChange: ({ value }) => (value?.trim() ? undefined : "First name is required"),
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							First name
						</label>

						<Input
							id={field.name}
							name={field.name}
							autoComplete="given-name"
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
				name="lastName"
				validators={{
					onChange: ({ value }) => (value?.trim() ? undefined : "Last name is required"),
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Last name
						</label>

						<Input
							id={field.name}
							name={field.name}
							autoComplete="family-name"
							value={field.state.value}
							disabled={isSubmitting}
							onBlur={field.handleBlur}
							onChange={(event) => field.handleChange(event.target.value)}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field name="phone">
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Phone
						</label>

						<Input
							id={field.name}
							name={field.name}
							type="tel"
							value={field.state.value}
							disabled={isSubmitting}
							onBlur={field.handleBlur}
							onChange={(event) => field.handleChange(event.target.value)}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
