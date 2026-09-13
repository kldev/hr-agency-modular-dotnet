import { useForm } from "@tanstack/react-form";
import type { ChangeInterviewFormatRequest } from "@/api/models";
import { EnumSelectFilter, FieldError, Textarea } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";
import { interviewFormats } from "@/features/interviews/type";

interface ChangeInterviewFormatFormProps {
	initialValue: ChangeInterviewFormatRequest;
	onSubmit: (value: ChangeInterviewFormatRequest) => void;
	error?: Error | null;
	formId: string;
	isSubmitting: boolean;
}

export const emptyFormat: ChangeInterviewFormatRequest = {
	format: "Online",
	note: "",
};

export function ChangeInterviewFormatForm({
	initialValue,
	onSubmit,
	error,
	formId,
	isSubmitting,
}: ChangeInterviewFormatFormProps) {
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
				name="format"
				validators={{
					onChange: ({ value }) => {
						if (!value) {
							return "Format is required";
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
							options={interviewFormats}
							onChange={(value) => {
								field.handleChange(value ?? "Online");
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
							autoFocus
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
