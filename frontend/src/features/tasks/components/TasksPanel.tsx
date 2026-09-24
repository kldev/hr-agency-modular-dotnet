import { ListTodo, Plus } from "lucide-react";
import type { TaskItemRow } from "#/api/models";
import { Button, EmptyState, EnumFilter } from "#/components/ui";
import { type TaskRangeParam, taskRanges, toApiRange } from "#/features/sales/workspace/search";
import { useTaskBoard, useToggleTask } from "../hooks/useTasks";
import { browserTimeZone } from "../relativeDay";
import { TaskRow } from "./TaskRow";
import "../tasks.css";

interface TasksPanelProps {
	range: TaskRangeParam;
	onRangeChange: (range: TaskRangeParam) => void;
	onAdd: () => void;
	onOpen: (task: TaskItemRow) => void;
}

/**
 * The caller's own tasks in every company - a salesperson's day does not stop at the company on
 * screen. Open ones on top, overdue included; done ones under them, muted. The period is cut by
 * the API, in the browser's time zone.
 */
export function TasksPanel({ range, onRangeChange, onAdd, onOpen }: TasksPanelProps) {
	const board = useTaskBoard({ range: toApiRange(range), timeZone: browserTimeZone() });
	const toggle = useToggleTask();

	const handleToggle = (task: TaskItemRow) => toggle.mutate({ task, done: task.status !== "Done" });

	const active = board.data?.active ?? [];
	const completed = board.data?.completed ?? [];
	const overdue = active.filter((task) => task.isOverdue).length;

	return (
		<section className="data-details-section tasks-panel" aria-label="Tasks">
			<div className="data-details-section-header">
				<div>
					<h2>Tasks</h2>
					<p>
						{active.length} to do{overdue ? `, ${overdue} overdue` : ""} · {completed.length} done
					</p>
				</div>

				<Button variant="secondary" icon={<Plus size={14} />} onClick={onAdd}>
					Add task
				</Button>
			</div>

			<div className="tasks-panel-range">
				<EnumFilter
					hideAll
					value={range}
					options={taskRanges}
					onChange={(value) => value && onRangeChange(value)}
				/>
			</div>

			{board.isLoading ? <div className="data-details-loading">Loading ...</div> : null}

			{board.isError ? (
				<div className="form-error" role="alert">
					Tasks could not be loaded.
				</div>
			) : null}

			{board.data ? (
				<>
					<h3 className="tasks-panel-title">Active</h3>

					{active.length === 0 ? (
						<EmptyState title="Nothing to do" description="No open tasks in this period.">
							<ListTodo size={22} />
						</EmptyState>
					) : (
						<ul className="task-list" aria-label="Active tasks">
							{active.map((task) => (
								<TaskRow key={task.id} task={task} onToggle={handleToggle} onOpen={onOpen} />
							))}
						</ul>
					)}

					<h3 className="tasks-panel-title">Completed</h3>

					{completed.length === 0 ? (
						<p className="form-hint tasks-panel-none">Nothing done in this period yet.</p>
					) : (
						<ul className="task-list" aria-label="Completed tasks">
							{completed.map((task) => (
								<TaskRow key={task.id} task={task} onToggle={handleToggle} />
							))}
						</ul>
					)}
				</>
			) : null}
		</section>
	);
}
