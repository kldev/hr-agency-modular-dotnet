import { useForm } from "@tanstack/react-form";
import type { ChangeJobApplicationStatusRequest } from "@/api/models";
import { EnumSelectFilter, FieldError, Textarea } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";
import { applicationStatuses } from "@/features/applications/types";

interface ChangeJobApplicationStatusFormProps {
	initialValue: ChangeJobApplicationStatusRequest;
	onSubmit: (value: ChangeJobApplicationStatusRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
}

export const emptyChangeJobApplicationStatus: ChangeJobApplicationStatusRequest = {
	status: "Assessment",
	note: null,
	interviewId: null,
};

export function ChangeJobApplicationStatusForm({
	initialValue,
	onSubmit,
	error,
	isSubmitting = false,
}: ChangeJobApplicationStatusFormProps) {
	const form = useForm({
		defaultValues: initialValue,

		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	return (
		<form
			id="change-job-application-status-form"
			className="drawer-form"
			onSubmit={(event) => {
				event.preventDefault();
				void form.handleSubmit();
			}}
		>
			<form.Field
				name="status"
				validators={{
					onChange: ({ value }) => {
						if (!value) {
							return "Status is required";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Status
						</label>

						<EnumSelectFilter
							hideAll
							value={field.state.value}
							options={applicationStatuses}
							onChange={(value) => {
								field.handleChange(value ?? "Assessment");
							}}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field name="note">
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Note
						</label>

						<Textarea
							id={field.name}
							name={field.name}
							value={field.state.value ?? ""}
							disabled={isSubmitting}
							onBlur={field.handleBlur}
							onChange={(event) => field.handleChange(event.target.value || null)}
							rows={5}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
