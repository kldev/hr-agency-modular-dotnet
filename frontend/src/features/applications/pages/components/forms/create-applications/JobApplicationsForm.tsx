import { useForm } from "@tanstack/react-form";
import type { ApplyToPostRequest } from "@/api/models";
import { EnumSelectFilter, FieldError, Input } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";
import { applicationSources } from "@/features/applications/types";

interface JobApplicationsFormProps {
	initialValue: ApplyToPostRequest;
	onSubmit: (value: ApplyToPostRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
}

export const emptyJobApplications: ApplyToPostRequest = {
	email: "",
	phoneNumber: "",
	firstName: "",
	lastName: "",
	source: undefined,
};

export function JobApplicationsForm({
	initialValue,
	onSubmit,
	error,
	isSubmitting = false,
}: JobApplicationsFormProps) {
	const form = useForm({
		defaultValues: initialValue,

		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	return (
		<form
			id="job-applications-form"
			className="drawer-form"
			onSubmit={(event) => {
				event.preventDefault();
				void form.handleSubmit();
			}}
		>
			<form.Field
				name="source"
				validators={{
					onChange: ({ value }) => (value?.trim() ? undefined : "Source is required"),
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Source
						</label>

						<EnumSelectFilter
							allLabel="Select source"
							value={field.state.value ?? null}
							options={applicationSources}
							onChange={(v) => field.handleChange(v ?? undefined)}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field
				name="email"
				validators={{
					onChange: ({ value }) => {
						if (!value.trim()) {
							return "Email is required";
						}

						if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
							return "Enter a valid email address";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Email
						</label>

						<Input
							id={field.name}
							name={field.name}
							autoComplete="email"
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

			<form.Field name="phoneNumber">
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
