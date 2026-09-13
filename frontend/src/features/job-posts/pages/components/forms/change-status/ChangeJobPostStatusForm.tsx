import { useForm } from "@tanstack/react-form";
import type { ChangeJobPostStatusRequest } from "@/api/models";
import { EnumSelectFilter, FieldError } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";
import { jobPostsStatuses } from "@/features/job-posts/type";

interface ChangeJobPostStatusFormProps {
	initialValue: ChangeJobPostStatusRequest;
	onSubmit: (value: ChangeJobPostStatusRequest) => void;
	error?: Error | null;
}

export const emptyChangeJobPostStatus: ChangeJobPostStatusRequest = {
	status: "Published",
};

export function ChangeJobPostStatusForm({
	initialValue,
	onSubmit,
	error,
}: ChangeJobPostStatusFormProps) {
	const form = useForm({
		defaultValues: initialValue,

		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	return (
		<form
			id="change-job-post-status-form"
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
							options={jobPostsStatuses}
							onChange={(value) => {
								field.handleChange(value ?? "Published");
							}}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
