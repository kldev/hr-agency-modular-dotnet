import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import type { TaskItemRow } from "#/api/models";
import { SaveChangesButton } from "#/components/ui";
import { Drawer } from "#/components/ui/Drawer";
import { useAuthStore } from "#/stores/authStore";
import { useSaveTask } from "../hooks/useTasks";
import { TaskForm, type TaskFormResult, type TaskFormValues, toDateTimeValue } from "./TaskForm";

export interface TaskDrawerRef {
	/** A new task, for the company on screen when there is one. */
	create: (company?: { id: string; name: string }) => void;
	edit: (task: TaskItemRow) => void;
}

type Target =
	| { mode: "create"; initial: TaskFormValues }
	| { mode: "edit"; id: string; initial: TaskFormValues };

const FORM_ID = "task-form";

/** Next full hour: a sensible default that is never already in the past. */
function nextHour(): Date {
	const date = new Date();
	date.setHours(date.getHours() + 1, 0, 0, 0);

	return date;
}

/**
 * One drawer for adding and changing a task - the API takes the same fields for both. The form is
 * mounted per target, so opening it again never shows what the previous task left behind.
 */
export const TaskDrawer = forwardRef<TaskDrawerRef, { onSaved?: () => void }>(
	({ onSaved }, ref) => {
		const { user } = useAuthStore();
		const [target, setTarget] = useState<Target | null>(null);

		const { mutation, waiting } = useSaveTask({
			onSuccess: () => {
				setTarget(null);
				mutation.reset();
				onSaved?.();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				create: (company) => {
					mutation.reset();
					setTarget({
						mode: "create",
						initial: {
							company: { id: company?.id ?? null, name: company?.name },
							opportunityId: "",
							title: "",
							description: "",
							dueAt: toDateTimeValue(nextHour()),
							priority: "Medium",
							assignee: { id: user?.userId ?? null },
						},
					});
				},
				edit: (task) => {
					mutation.reset();
					setTarget({
						mode: "edit",
						id: task.id,
						initial: {
							company: { id: task.company.id, name: task.company.name },
							opportunityId: task.opportunity?.id ?? "",
							title: task.title,
							description: task.description ?? "",
							dueAt: toDateTimeValue(task.dueAt),
							priority: task.priority,
							assignee: { id: task.assigneeId },
						},
					});
				},
			}),
			[mutation, user?.userId],
		);

		const handleSubmit = useCallback(
			(value: TaskFormResult) => {
				if (!target) return;

				const common = {
					opportunityId: value.opportunityId,
					title: value.title,
					description: value.description,
					dueAt: value.dueAt,
					priority: value.priority,
				};

				if (target.mode === "create") {
					mutation.mutate({
						mode: "create",
						request: { ...common, companyId: value.companyId, assigneeId: value.assigneeId },
					});
					return;
				}

				mutation.mutate({
					mode: "edit",
					id: target.id,
					request: { ...common, assigneeId: value.assigneeId ?? (user?.userId as string) },
				});
			},
			[mutation, target, user?.userId],
		);

		const handleClose = useCallback(() => {
			if (mutation.isPending) return;

			mutation.reset();
			setTarget(null);
		}, [mutation]);

		return (
			<Drawer
				open={target !== null}
				title={target?.mode === "edit" ? "Edit task" : "New task"}
				onClose={handleClose}
				footer={<SaveChangesButton form={FORM_ID} isPending={mutation.isPending} wait={waiting} />}
			>
				{target ? (
					<TaskForm
						key={target.mode === "edit" ? target.id : "new"}
						formId={FORM_ID}
						initial={target.initial}
						companyLocked={target.mode === "edit"}
						onSubmit={handleSubmit}
						error={mutation.error}
						isSubmitting={mutation.isPending}
					/>
				) : null}
			</Drawer>
		);
	},
);

TaskDrawer.displayName = "TaskDrawer";
