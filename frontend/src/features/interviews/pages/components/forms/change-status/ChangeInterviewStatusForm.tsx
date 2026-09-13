import { useForm } from "@tanstack/react-form";
import type { ChangeInterviewStatusRequest } from "@/api/models";
import { EnumSelectFilter, FieldError, Textarea } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";
import { interviewStatuses } from "@/features/interviews/type";

interface ChangeInterviewStatusFormProps {
	initialValue: ChangeInterviewStatusRequest;
	onSubmit: (value: ChangeInterviewStatusRequest) => void;
	error?: Error | null;
	formId: string;
	isSubmitting: boolean
}

export const empty: ChangeInterviewStatusRequest = {
	status: "Planned",
	note: "",
};

export function ChangeInterviewStatusForm({
	initialValue,
	onSubmit,
	error,
	formId,
	isSubmitting
}: ChangeInterviewStatusFormProps) {
	const form = useForm({
		defaultValues: initialValue,

		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	return (
		<form
			id={formId}
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
							options={interviewStatuses}
							onChange={(value) => {
								field.handleChange(value ?? "Planned");
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
							onChange={(event) => field.handleChange(event.target.value)}
							rows={7}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>


			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
