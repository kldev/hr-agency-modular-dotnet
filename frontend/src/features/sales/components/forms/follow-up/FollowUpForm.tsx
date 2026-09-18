import { format } from "date-fns";
import { z } from "zod";
import { ApiError } from "#/components/ui/ApiError";
import { type FormDateTimeValue, useAppForm } from "#/forms";

interface FollowUpFormProps {
	initial: FollowUpFormValues;
	onSubmit: (value: { content: string; followDateTime: string }) => void;
	error?: Error | null;
	isSubmitting: boolean;
	formId: string;
}

export type FollowUpFormValues = {
	content: string;
	followDateTime: FormDateTimeValue;
};

const followUpSchema = z.object({
	content: z.string().min(1, "Content is required."),
	followDateTime: z.object({
		date: z.date().nullable(),
		time: z.string(),
	}),
});

export function emptyFollowUp(): FollowUpFormValues {
	return {
		content: "",
		followDateTime: { date: null, time: "" },
	};
}

export function toFormValues(content: string, followDateTime: string): FollowUpFormValues {
	const value = new Date(followDateTime);

	return {
		content,
		followDateTime: { date: value, time: format(value, "HH:mm") },
	};
}

function toIsoDateTime({ date, time }: FormDateTimeValue): string {
	const [hours, minutes] = time.split(":").map(Number);
	const value = new Date(date as Date);

	value.setHours(hours, minutes, 0, 0);

	return value.toISOString();
}

export function FollowUpForm({
	initial,
	onSubmit,
	error,
	isSubmitting,
	formId,
}: FollowUpFormProps) {
	const form = useAppForm({
		defaultValues: initial,

		validators: {
			onChange: followUpSchema,
		},

		onSubmit: async ({ value }) => {
			onSubmit({
				content: value.content,
				followDateTime: toIsoDateTime(value.followDateTime),
			});
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
			<form.AppField
				name="followDateTime"
				validators={{
					onChange: ({ value }) => {
						if (!value.date) return "Date is required";
						if (!value.time) return "Time is required";

						return undefined;
					},
				}}
			>
				{(field) => (
					<field.FormDateTime
						fieldValue={field.state.value}
						label="Follow up at"
						errors={field.state.meta.errors}
						fieldName={field.name}
						handleChange={(val) => field.handleChange(val)}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>

			<form.AppField name="content">
				{(field) => (
					<field.FormTextAreaInput
						fieldValue={field.state.value}
						label="Content"
						errors={field.state.meta.errors}
						fieldName={field.name}
						handleChange={(val) => field.handleChange(val)}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>

			{isConflict(error) ? (
				<div className="form-error" role="alert">
					<strong>A follow up with this date and time already exists for the opportunity.</strong>

					<div className="text-xl text-muted!">Pick a different time.</div>
				</div>
			) : (
				<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
			)}
		</form>
	);
}

// the unique (opportunityId, followDateTime) index answers with 409, and the status survives
// the server function boundary either on the error itself or on the serialized response
function isConflict(error?: Error | null): boolean {
	if (!error || typeof error !== "object") {
		return false;
	}

	const candidate = error as {
		status?: number;
		response?: { status?: number; data?: { status?: number } };
	};

	return (
		candidate.status === 409 ||
		candidate.response?.status === 409 ||
		candidate.response?.data?.status === 409
	);
}
