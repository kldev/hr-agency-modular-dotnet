import { useForm } from "@tanstack/react-form";
import type { CreateNoteRequest } from "@/api/models";
import { FieldError, Textarea } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";

interface AddJobApplicationNoteFormProps {
	initialValue: CreateNoteRequest;
	onSubmit: (value: CreateNoteRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
}

export const emptyCreateNote: CreateNoteRequest = {
	note: "",
};

export function AddJobApplicationNoteForm({
	initialValue,
	onSubmit,
	error,
	isSubmitting = false,
}: AddJobApplicationNoteFormProps) {
	const form = useForm({
		defaultValues: initialValue,

		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	return (
		<form
			id="add-job-application-note-form"
			className="drawer-form"
			onSubmit={(event) => {
				event.preventDefault();
				void form.handleSubmit();
			}}
		>
			<form.Field
				name="note"
				validators={{
					onChange: ({ value }) => {
						if (!value.trim()) {
							return "Note is required";
						}

						return undefined;
					},
				}}
			>
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
