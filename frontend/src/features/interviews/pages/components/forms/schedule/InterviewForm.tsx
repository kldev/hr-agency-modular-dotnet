import { formatISO } from "date-fns";

import { type FormDateTimeValue, useAppForm } from "#/forms";
import type { InterviewFormat, InterviewType, ScheduleInterviewRequest } from "@/api/models";

import { ApiError } from "@/components/ui/ApiError";
import { interviewFormats, interviewTypes } from "@/features/interviews/type";
import { parseScheduledAt } from "@/features/interviews/utils/parseScheduledAt";
import { formatLocalDateTime, getBrowserTimezone } from "@/utlis/formatLocalDateTime";

interface InterviewFormProps {
	onSubmit: (value: ScheduleInterviewRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
}

interface ScheduleIntervieFormValues {
	scheduledAt: FormDateTimeValue;
	note: string;
	scheduledTimezone?: string;
	location?: string;
	meetingUrl?: string;
	format: InterviewFormat;
	interviewType: InterviewType;
	interviewerId: string;
}

const emptyScheduleInterview: ScheduleIntervieFormValues = {
	format: "Online",
	interviewerId: "",
	interviewType: "Hr",
	note: "",
	scheduledAt: { date: null, time: "" },
	location: "",
	meetingUrl: "",
};

export function InterviewForm({ onSubmit, error, isSubmitting = false }: InterviewFormProps) {
	const intialDate = new Date();
	const initialScheduled = parseScheduledAt(formatISO(intialDate));

	const intialFormValue: ScheduleIntervieFormValues = {
		...emptyScheduleInterview,
		scheduledAt: { date: initialScheduled.date, time: initialScheduled.time },
	};

	const form = useAppForm({
		defaultValues: {
			...intialFormValue,
		},

		onSubmit: async ({ value }) => {
			onSubmit({
				...value,
				scheduledAt: formatLocalDateTime(value.scheduledAt.date as Date, value.scheduledAt.time),
				scheduledTimezone: getBrowserTimezone(),
				jobApplicationId: "",
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
			<form.AppField
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
					<field.FormUserPicker
						placeholder="Search interviewer"
						fieldValue={{ id: field.state.value }}
						label="Interviewer"
						errors={field.state.meta.errors}
						fieldName={field.name}
						handleChange={(val) => field.handleChange(val.id ?? "")}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>

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
						label="Description"
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
						label="Description"
						errors={field.state.meta.errors}
						fieldName={field.name}
						handleChange={(val) => field.handleChange(val)}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>

			<form.AppField name="interviewType">
				{(field) => (
					<field.FormSelectEnum
						options={interviewTypes}
						fieldValue={field.state.value ?? ""}
						label="Type"
						errors={field.state.meta.errors}
						fieldName={field.name}
						handleChange={(val) => field.handleChange(val)}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>

			<form.AppField name="format">
				{(field) => (
					<field.FormSelectEnum
						options={interviewFormats}
						fieldValue={field.state.value ?? ""}
						label="Format"
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
