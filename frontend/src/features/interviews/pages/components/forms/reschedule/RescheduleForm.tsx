import { useMemo, useState } from "react";
import { useAppForm } from "#/forms";
import type { RescheduleInterviewRequest } from "@/api/models";
import { DatePicker, TimeInput } from "@/components/ui";
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
	const [scheduledDate, setScheduledDate] = useState<Date | null>(initialScheduled.date);

	const [scheduledTime, setScheduledTime] = useState<string>(initialScheduled.time);

	const form = useAppForm({
		defaultValues: initialValue,

		onSubmit: async ({ value }) => {
			if (!scheduledDate || !scheduledTime) {
				return;
			}

			onSubmit({
				...value,
				scheduledAt: formatLocalDateTime(scheduledDate, scheduledTime),
				scheduledTimezone: value.scheduledTimezone || getBrowserTimezone(),
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
			<div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
				<div className="form-field">
					<span className="form-label">Date</span>

					<DatePicker
						value={scheduledDate}
						onChange={setScheduledDate}
						disabled={isSubmitting}
						clearable
					/>

					{!scheduledDate && (
						<div className="mt-1 text-xs text-(--color-danger)">Date is required</div>
					)}
				</div>

				<div className="form-field">
					<span className="form-label">Time</span>

					<TimeInput value={scheduledTime} onChange={setScheduledTime} disabled={isSubmitting} />

					{!scheduledTime && (
						<div className="mt-1 text-xs text-(--color-danger)">Time is required</div>
					)}
				</div>
			</div>

			<form.AppField name="note">
				{(field) => (
					<field.FormTextAreaInput
						label="Description"
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
