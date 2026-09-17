import { useForm } from "@tanstack/react-form";
import { useState } from "react";
import type { AssignRecruiterRequest } from "@/api/models";
import { FieldError, UsersPicker } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";

interface ChangeRecruiterFormProps {
	initialValue: AssignRecruiterRequest;
	onSubmit: (value: AssignRecruiterRequest) => void;
	error?: Error | null;
	formId: string;
	isSubmitting: boolean;
}

export const emptyForm: AssignRecruiterRequest = {
	recruiterId: "",
};

export function ChangeRecruiterForm({
	initialValue,
	onSubmit,
	error,
	formId,
	isSubmitting,
}: ChangeRecruiterFormProps) {
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
				name="recruiterId"
				validators={{
					onChange: ({ value }) => {
						if (!value.trim()) {
							return "Recruiter is required";
						}

						if (value === initialValue.recruiterId) {
							return "This recruiter is already responsible for the job description";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Recruiter
						</label>

						<UsersPicker
							label="Select recruiter"
							placeholder="Search recruiter"
							disabled={isSubmitting}
							inputValue={input}
							onInputChange={setInput}
							value={field.state.value}
							onChange={(value) => field.handleChange(value ?? "")}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
