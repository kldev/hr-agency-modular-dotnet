import { useForm } from "@tanstack/react-form";
import { useMemo, useState } from "react";
import type { RescheduleInterviewRequest } from "@/api/models";
import { DatePicker, FieldError, Textarea, TimeInput } from "@/components/ui";
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

	const form = useForm({
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

			<form.Field name="note">
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
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
