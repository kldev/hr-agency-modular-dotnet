import { useForm } from "@tanstack/react-form";

import { useMemo, useState } from "react";
import type { ScheduleInterviewRequest } from "@/api/models";
import {
	DatePicker,
	EnumSelectFilter,
	FieldError,
	Textarea,
	TimeInput,
	UsersPicker,
} from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";
import { interviewFormats, interviewTypes } from "@/features/interviews/type";
import { formatLocalDateTime, getBrowserTimezone } from "@/utlis/formatLocalDateTime";

interface InterviewFormProps {
	initialValue: ScheduleInterviewRequest;
	onSubmit: (value: ScheduleInterviewRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
}

function parseScheduledAt(value: string): {
	date: Date | null;
	time: string;
} {
	if (!value) {
		return {
			date: null,
			time: "",
		};
	}

	const [datePart, timePart] = value.split("T");

	if (!datePart) {
		return {
			date: null,
			time: "",
		};
	}

	const [year, month, day] = datePart.split("-").map(Number);

	if (!year || !month || !day) {
		return {
			date: null,
			time: "",
		};
	}

	return {
		date: new Date(year, month - 1, day),
		time: timePart?.slice(0, 5) ?? "",
	};
}

export const emptyScheduleInterview: ScheduleInterviewRequest = {
	format: "Online",
	interviewerId: "",
	interviewType: "Hr",
	jobApplicationId: "",
	note: "",
	scheduledAt: "",
	scheduledTimezone: getBrowserTimezone(),
};

export function InterviewForm({
	initialValue,
	onSubmit,
	error,
	isSubmitting = false,
}: InterviewFormProps) {
	const initialScheduled = useMemo(
		() => parseScheduledAt(initialValue.scheduledAt),
		[initialValue.scheduledAt],
	);

	const [scheduledDate, setScheduledDate] = useState<Date | null>(initialScheduled.date);

	const [scheduledTime, setScheduledTime] = useState<string>(initialScheduled.time);

	const [input, setInput] = useState<string>("");

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
			id="interview-form"
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

			<form.Field name="interviewType">
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Type
						</label>

						<EnumSelectFilter
							hideAll
							value={field.state.value}
							options={interviewTypes}
							onChange={(value) => {
								if (value) {
									field.handleChange(value);
								}
							}}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field name="format">
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Format
						</label>

						<EnumSelectFilter
							hideAll
							value={field.state.value}
							options={interviewFormats}
							onChange={(value) => {
								if (value) {
									field.handleChange(value);
								}
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
