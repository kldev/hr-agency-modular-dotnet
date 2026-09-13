import { useForm } from "@tanstack/react-form";
import type { PostToChannelRequest } from "@/api/models";
import { EnumSelectFilter, FieldError } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";
import { jobPostChannels } from "@/features/job-posts/type";

interface PostToChannelFormProps {
	initialValue: PostToChannelRequest;
	onSubmit: (value: PostToChannelRequest) => void;
	error?: Error | null;
}

export const emptyRequest: PostToChannelRequest = {
	channel: "CareerPage",
};

export function PostToChannelForm({ initialValue, onSubmit, error }: PostToChannelFormProps) {
	const form = useForm({
		defaultValues: initialValue,

		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	return (
		<form
			id="post-to-channel-form"
			className="drawer-form"
			onSubmit={(event) => {
				event.preventDefault();
				void form.handleSubmit();
			}}
		>
			<form.Field
				name="channel"
				validators={{
					onChange: ({ value }) => {
						if (!value) {
							return "Channel is required";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Channel
						</label>

						<EnumSelectFilter
							hideAll
							value={field.state.value}
							options={jobPostChannels}
							onChange={(value) => {
								field.handleChange(value ?? "CareerPage");
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
