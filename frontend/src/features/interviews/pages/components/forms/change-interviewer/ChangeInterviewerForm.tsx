import { useForm } from "@tanstack/react-form";
import { useState } from "react";
import type { ChangeInterviewerRequest } from "@/api/models";
import { FieldError, Textarea, UsersPicker } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";

interface ChangeInterviewerFormProps {
	initialValue: ChangeInterviewerRequest;
	onSubmit: (value: ChangeInterviewerRequest) => void;
	error?: Error | null;
	formId: string;
	isSubmitting: boolean;
}

export const emptyForm: ChangeInterviewerRequest = {
	interviewerId: "",
	note: "",
};

export function ChangeInterviewerForm({
	initialValue,
	onSubmit,
	error,
	formId,
	isSubmitting,
}: ChangeInterviewerFormProps) {
	const [input, setInput] = useState<string>("");
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
				name="interviewerId"
				validators={{
					onChange: ({ value }) => {
						if (!value.trim()) {
							return "Interviewer is required";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Interviewer
						</label>

						<UsersPicker
							inputValue={input}
							onInputChange={setInput}
							value={field.state.value}
							onChange={(value) => field.handleChange(value ?? "")}
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
