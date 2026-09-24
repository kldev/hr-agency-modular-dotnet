import { format } from "date-fns";
import { z } from "zod";
import type { TaskPriority } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { OpportunitySelect } from "#/features/sales/components/OpportunitySelect";
import { type FormDateTimeValue, useAppForm } from "#/forms";
import { taskPriorities } from "../types";

export type TaskFormValues = {
	company: { id: string | null; name?: string };
	opportunityId: string;
	title: string;
	description: string;
	dueAt: FormDateTimeValue;
	priority: TaskPriority;
	assignee: { id: string | null };
};

export type TaskFormResult = {
	companyId: string;
	opportunityId: string | null;
	title: string;
	description: string | null;
	dueAt: string;
	priority: TaskPriority;
	assigneeId: string | null;
};

// Mirrors TaskTitle and TaskItemInputValidator; the API has the last word.
const titleSchema = z
	.string()
	.trim()
	.min(1, "Task title is required.")
	.max(200, "Task title cannot exceed 200 characters.");

const descriptionSchema = z.string().max(2000, "A description cannot exceed 2000 characters.");

export function toDateTimeValue(value: string | Date): FormDateTimeValue {
	const date = new Date(value);

	return { date, time: format(date, "HH:mm") };
}

function toIso({ date, time }: FormDateTimeValue): string {
	const [hours, minutes] = time.split(":").map(Number);
	const value = new Date(date as Date);

	value.setHours(hours, minutes, 0, 0);

	return value.toISOString();
}

interface TaskFormProps {
	formId: string;
	initial: TaskFormValues;
	/** Editing: the company is fixed, a task for another company is another task. */
	companyLocked?: boolean;
	onSubmit: (value: TaskFormResult) => void;
	error?: Error | null;
	isSubmitting: boolean;
}

export function TaskForm({
	formId,
	initial,
	companyLocked,
	onSubmit,
	error,
	isSubmitting,
}: TaskFormProps) {
	const form = useAppForm({
		defaultValues: initial,

		onSubmit: async ({ value }) => {
			onSubmit({
				companyId: value.company.id as string,
				opportunityId: value.opportunityId || null,
				title: value.title.trim(),
				description: value.description.trim() || null,
				dueAt: toIso(value.dueAt),
				priority: value.priority,
				assigneeId: value.assignee.id,
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
			{companyLocked ? (
				<div className="form-field">
					<span className="form-label">Company</span>
					<p className="form-wizard__readonly-value">{initial.company.name}</p>
				</div>
			) : (
				<form.AppField
					name="company"
					validators={{
						onChange: ({ value }) => (value.id ? undefined : "Pick the company the task is for."),
					}}
				>
					{(field) => (
						<field.FormCompanyPicker
							label="Company"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							isSubmitting={isSubmitting}
							handleChange={(value) => {
								field.handleChange({ id: value.id, name: value.company?.name });
								form.setFieldValue("opportunityId", "");
							}}
						/>
					)}
				</form.AppField>
			)}

			<form.Subscribe selector={(state) => state.values.company.id}>
				{(companyId) => (
					<form.AppField name="opportunityId">
						{(field) => (
							<OpportunitySelect
								companyId={companyId}
								value={field.state.value}
								onChange={field.handleChange}
								errors={field.state.meta.errors}
								isSubmitting={isSubmitting}
								fieldName={field.name}
							/>
						)}
					</form.AppField>
				)}
			</form.Subscribe>

			<form.AppField name="title" validators={{ onChange: titleSchema }}>
				{(field) => (
					<field.FormInput
						label="Title"
						fieldName={field.name}
						fieldValue={field.state.value}
						errors={field.state.meta.errors}
						isSubmitting={isSubmitting}
						handleChange={field.handleChange}
					/>
				)}
			</form.AppField>

			<form.AppField
				name="dueAt"
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
						label="Due"
						fieldName={field.name}
						fieldValue={field.state.value}
						errors={field.state.meta.errors}
						isSubmitting={isSubmitting}
						handleChange={field.handleChange}
					/>
				)}
			</form.AppField>

			<form.AppField name="priority">
				{(field) => (
					<field.FormSelectEnum
						label="Priority"
						fieldName={field.name}
						fieldValue={field.state.value}
						options={taskPriorities}
						errors={field.state.meta.errors}
						isSubmitting={isSubmitting}
						handleChange={field.handleChange}
					/>
				)}
			</form.AppField>

			<form.AppField name="assignee">
				{(field) => (
					<field.FormUserPicker
						label="Assignee"
						fieldName={field.name}
						fieldValue={field.state.value}
						errors={field.state.meta.errors}
						isSubmitting={isSubmitting}
						handleChange={(value) => field.handleChange({ id: value.id })}
					/>
				)}
			</form.AppField>

			<form.AppField name="description" validators={{ onChange: descriptionSchema }}>
				{(field) => (
					<field.FormTextAreaInput
						label="Details"
						fieldName={field.name}
						fieldValue={field.state.value}
						errors={field.state.meta.errors}
						isSubmitting={isSubmitting}
						handleChange={field.handleChange}
					/>
				)}
			</form.AppField>

			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
