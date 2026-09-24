import clsx from "clsx";
import { Check } from "lucide-react";
import type { TaskItemRow } from "#/api/models";
import { TaskPriorityBadge } from "#/components/ui";
import { relativeDayLabel } from "../relativeDay";

interface TaskRowProps {
	task: TaskItemRow;
	onToggle: (task: TaskItemRow) => void;
	onOpen?: (task: TaskItemRow) => void;
	disabled?: boolean;
}

/**
 * One task: the tick on the left is the whole point of the list - a 44 px target, so it works under
 * a thumb as well as a mouse. The title opens the task; a done one is only reopened, not edited.
 */
export function TaskRow({ task, onToggle, onOpen, disabled }: TaskRowProps) {
	const done = task.status === "Done";

	return (
		<li className={clsx("task-row", { "is-done": done, "is-overdue": task.isOverdue })}>
			<label className="task-check">
				<input
					type="checkbox"
					className="task-check-input"
					checked={done}
					aria-label={done ? `Reopen ${task.title}` : `Mark ${task.title} as done`}
					disabled={disabled}
					onChange={() => onToggle(task)}
				/>
				<span className="task-check-box" aria-hidden="true">
					{done ? <Check size={14} strokeWidth={3} /> : null}
				</span>
			</label>

			<div className="task-body">
				{onOpen && !done ? (
					<button type="button" className="task-title" onClick={() => onOpen(task)}>
						{task.title}
					</button>
				) : (
					<span className="task-title">{task.title}</span>
				)}

				<div className="task-meta">
					<span className="truncate">
						{task.company.name}
						{task.opportunity ? ` · ${task.opportunity.name}` : ""}
					</span>
				</div>

				<div className="task-meta">
					<span className="task-due">
						{done && task.completedAt
							? relativeDayLabel(task.completedAt)
							: relativeDayLabel(task.dueAt)}
						{task.isOverdue ? " · overdue" : ""}
					</span>
				</div>
			</div>

			{done ? null : <TaskPriorityBadge priority={task.priority} />}
		</li>
	);
}
