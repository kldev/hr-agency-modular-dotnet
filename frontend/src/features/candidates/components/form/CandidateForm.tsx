import { useForm } from "@tanstack/react-form";
import type { CreateCandidateRequest, UpdateCandidateRequest } from "@/api/models";
import { EnumSelectFilter, FieldError, Input, Textarea } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";
import { applicationSources } from "@/features/applications/types";

interface CandidateFormProps {
	initialValue: CreateCandidateRequest | UpdateCandidateRequest;
	mode: "create" | "edit";
	onSubmit: (value: CreateCandidateRequest | UpdateCandidateRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
}

export const emptyCreateCandidate: CreateCandidateRequest = {
	email: "",
	phoneNumber: "",
	firstName: "",
	lastName: "",
	source: "InternalDatabase",
	note: "",
};

export const emptyUpdateCandidate: UpdateCandidateRequest = {
	phone: "",
	firstName: "",
	lastName: "",
	note: "",
};

export function CandidateForm({
	initialValue,
	mode,
	onSubmit,
	error,
	isSubmitting = false,
}: CandidateFormProps) {
	const isEdit = mode === "edit";

	const form = useForm({
		defaultValues: initialValue,

		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	return (
		<form
			id="candidate-form"
			className="drawer-form"
			onSubmit={(event) => {
				event.preventDefault();
				void form.handleSubmit();
			}}
		>
			{!isEdit && (
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
								type="email"
								value={field.state.value}
								disabled={isSubmitting}
								onBlur={field.handleBlur}
								onChange={(event) => field.handleChange(event.target.value)}
							/>

							<FieldError errors={field.state.meta.errors} />
						</div>
					)}
				</form.Field>
			)}

			<form.Field
				name="firstName"
				validators={{
					onChange: ({ value }) => {
						if (!value.trim()) {
							return "First name is required";
						}

						return undefined;
					},
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
					onChange: ({ value }) => {
						if (!value.trim()) {
							return "Last name is required";
						}

						return undefined;
					},
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
							value={field.state.value}
							disabled={isSubmitting}
							onBlur={field.handleBlur}
							onChange={(event) => field.handleChange(event.target.value)}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field name={isEdit ? "phone" : "phoneNumber"}>
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

			{!isEdit && (
				<form.Field name="source">
					{(field) => (
						<div className="form-field">
							<label className="form-label" htmlFor={field.name}>
								Source
							</label>

							<EnumSelectFilter
								hideAll
								value={field.state.value}
								options={applicationSources}
								onChange={(value) => field.handleChange(value ?? "InternalDatabase")}
							/>

							<FieldError errors={field.state.meta.errors} />
						</div>
					)}
				</form.Field>
			)}

			<form.Field name="note">
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Note
						</label>

						<Textarea
							id={field.name}
							name={field.name}
							value={field.state.value}
							disabled={isSubmitting}
							onBlur={field.handleBlur}
							onChange={(event) => field.handleChange(event.target.value)}
							rows={5}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			{isEdit && (
				<div className="form-field">
					<label className="form-label" htmlFor="candidate-email">
						Email
					</label>

					<Input
						id="candidate-email"
						value={(initialValue as CreateCandidateRequest).email}
						disabled
						readOnly
					/>

					<div className="form-hint">Email cannot be changed after the candidate is created.</div>
				</div>
			)}

			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
