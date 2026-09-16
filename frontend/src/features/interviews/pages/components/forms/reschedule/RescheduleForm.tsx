import { useMemo } from "react";
import { type FormDateTimeValue, useAppForm } from "#/forms";
import type { RescheduleInterviewRequest } from "@/api/models";
import { ApiError } from "@/components/ui/ApiError";
import { parseScheduledAt } from "@/features/interviews/utils";
import { formatLocalDateTime, getBrowserTimezone } from "@/utlis/formatLocalDateTime";

interface RescheduleFormProps {
	initialValue: RescheduleInterviewRequest;
	onSubmit: (value: RescheduleInterviewRequest) => void;
	error?: Error | null;
	formId: string;
	isSubmitting: boolean;
}

interface RescheduleIntervieFormValues {
	scheduledAt: FormDateTimeValue;
	note: string;
	scheduledTimezone?: string;
	location?: string;
	meetingUrl?: string;
}

export function RescheduleForm({
	initialValue,
	onSubmit,
	error,
	formId,
	isSubmitting,
}: RescheduleFormProps) {
	const initialScheduled = useMemo(
		() => parseScheduledAt(initialValue.scheduledAt),
		[initialValue.scheduledAt],
	);

	const intialFormValue: RescheduleIntervieFormValues = {
		note: initialValue.note,
		meetingUrl: initialValue.meetingUrl,
		scheduledAt: {
			date: initialScheduled.date,
			time: initialScheduled.time,
		},
		location: initialValue.location,
	};

	const form = useAppForm({
		defaultValues: intialFormValue,

		onSubmit: async ({ value }) => {
			console.log(`On submit ${JSON.stringify(value.scheduledAt)}`);
			onSubmit({
				...value,
				scheduledAt: formatLocalDateTime(value.scheduledAt.date as Date, value.scheduledAt.time),
				scheduledTimezone: getBrowserTimezone(),
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
				name="scheduledAt"
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
						label="Schedule at"
						errors={field.state.meta.errors}
						fieldName={field.name}
						handleChange={(val) => field.handleChange(val)}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>

			<form.AppField name="note">
				{(field) => (
					<field.FormTextAreaInput
						fieldValue={field.state.value}
						label="Note"
						errors={field.state.meta.errors}
						fieldName={field.name}
						handleChange={(val) => field.handleChange(val)}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>

			<form.AppField name="location">
				{(field) => (
					<field.FormInput
						fieldValue={field.state.value ?? ""}
						label="Location"
						errors={field.state.meta.errors}
						fieldName={field.name}
						handleChange={(val) => field.handleChange(val)}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>

			<form.AppField name="meetingUrl">
				{(field) => (
					<field.FormInput
						fieldValue={field.state.value ?? ""}
						label="Meeting url"
						errors={field.state.meta.errors}
						fieldName={field.name}
						handleChange={(val) => field.handleChange(val)}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>

			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
