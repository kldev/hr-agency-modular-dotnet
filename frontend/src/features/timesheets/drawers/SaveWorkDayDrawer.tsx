import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { Button, Select } from "@/components/ui";
import { useRemoveWorkDay, useSaveWorkDay } from "../pages/hooks";
import { formatMinutes, fromDateKey, hourOptions, minuteOptions, toTimeOfDay } from "../types";
import type { SaveWorkDayFormCommand, WorkDayTarget } from "./TimeSheetFormCommand";

interface Props {
	onSuccess: () => void;
}

const schema = z
	.object({
		startsAt: z.string().min(1, "Say when the day began"),
		hours: z.string(),
		minutes: z.string(),
		note: z.string().max(500, "Note cannot exceed 500 characters."),
	})
	.refine((value) => Number(value.hours) > 0 || Number(value.minutes) > 0, {
		path: ["hours"],
		/* Mirrors `WorkDuration.EmptyMessage`: a day with no time on it is not an entry. */
		message: "A day with no time on it is not an entry. Remove the day instead.",
	})
	.refine((value) => Number(value.hours) * 60 + Number(value.minutes) <= 24 * 60, {
		path: ["hours"],
		/* Mirrors `WorkDuration.TooLongMessage` - 24 h plus five minutes is the only way to hit it. */
		message: "A single day cannot hold more than 24 hours.",
	});

const FormContent: React.FC<{
	target: WorkDayTarget;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ target, onSuccess, handleClose }) => {
	const { mutation, waiting } = useSaveWorkDay({
		onSuccess: () => {
			mutation.reset();
			toast.success("Day saved");
			onSuccess();
			handleClose();
		},
	});

	const removal = useRemoveWorkDay({
		onSuccess: () => {
			toast.success("Day removed");
			onSuccess();
			handleClose();
		},

		onError: () => toast.error("The day could not be removed"),
	});

	const existing = target.day;

	const form = useAppForm({
		defaultValues: {
			startsAt: existing ? toTimeOfDay(existing.startsAt) : "08:00",
			hours: existing ? String(Math.floor(Number(existing.minutes) / 60)) : "8",
			minutes: existing ? String(Number(existing.minutes) % 60) : "0",
			note: existing?.note ?? "",
		},

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				request: {
					date: target.date,
					startsAt: value.startsAt,
					hours: Number(value.hours),
					minutes: Number(value.minutes),
					note: value.note.trim() === "" ? null : value.note.trim(),
				},
			});
		},
	});

	const dayLabel = fromDateKey(target.date).toLocaleDateString(undefined, {
		weekday: "long",
		day: "numeric",
		month: "long",
	});

	const pending = mutation.isPending || removal.mutation.isPending;

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				/* The date is the identity of the entry, so it is the title rather than a field. */
				title={dayLabel}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField name="startsAt">
							{(field) => (
								<field.FormTimeInput
									label="Started at"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={pending}
									/* A night shift is a work day; it simply ends on the next one. */
									fromMinutes={0}
									toMinutes={23 * 60 + 55}
								/>
							)}
						</form.AppField>

						{/*
						 * Length, not an end time: people think "I came in at eight and worked eight and
						 * a half", and the end is arithmetic the backend does anyway. Minutes are a list
						 * rather than a number field, so "steps of five" is the shape of the form instead
						 * of a refusal after the fact.
						 */}
						<div className="form-field">
							<span className="form-label">Worked</span>

							<div className="grid grid-cols-2 gap-3">
								<form.AppField name="hours">
									{(field) => (
										<Select
											id={field.name}
											name={field.name}
											value={field.state.value}
											disabled={pending}
											aria-label="Hours"
											onChange={(event) => field.handleChange(event.target.value)}
										>
											{hourOptions.map((hour) => (
												<option key={hour} value={hour}>
													{hour} h
												</option>
											))}
										</Select>
									)}
								</form.AppField>

								<form.AppField name="minutes">
									{(field) => (
										<Select
											id={field.name}
											name={field.name}
											value={field.state.value}
											disabled={pending}
											aria-label="Minutes"
											onChange={(event) => field.handleChange(event.target.value)}
										>
											{minuteOptions.map((minute) => (
												<option key={minute} value={minute}>
													{minute} min
												</option>
											))}
										</Select>
									)}
								</form.AppField>
							</div>

							<form.Subscribe
								selector={(state) => [state.values.hours, state.values.minutes] as const}
							>
								{([hours, minutes]) => {
									const total = Number(hours) * 60 + Number(minutes);

									return total > 12 * 60 ? (
										<div className="form-hint">
											{formatMinutes(total)} on one day. Allowed, and worth a second look.
										</div>
									) : null;
								}}
							</form.Subscribe>
						</div>

						<form.AppField name="note">
							{(field) => (
								<field.FormTextAreaInput
									label="Note"
									rows={3}
									placeholder="Optional"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={pending}
								/>
							)}
						</form.AppField>

						<ApiError
							error={mutation.error as unknown as Parameters<typeof ApiError>[0]["error"]}
						/>
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					{existing ? (
						<Button
							variant="danger"
							type="button"
							loading={removal.mutation.isPending || removal.waiting}
							onClick={() => removal.mutation.mutate({ date: target.date })}
						>
							Remove day
						</Button>
					) : null}

					<form.FormSaveChangesButton wait={waiting} isPending={pending} />
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
};

const SaveWorkDayDrawer = forwardRef<SaveWorkDayFormCommand, Props>(({ onSuccess }, ref) => {
	const [target, setTarget] = useState<WorkDayTarget | null>(null);

	useImperativeHandle(ref, () => ({ saveDay: setTarget }), []);

	if (!target) return null;

	return <FormContent target={target} onSuccess={onSuccess} handleClose={() => setTarget(null)} />;
});

SaveWorkDayDrawer.displayName = "SaveWorkDayDrawer";

export default SaveWorkDayDrawer;
