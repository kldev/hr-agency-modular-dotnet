import { type QueryKey, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import {
	completeTask,
	createTask,
	getTask,
	getTaskBoard,
	reopenTask,
	updateTask,
} from "@/api/endpoints";
import type {
	CreateTaskRequest,
	TaskBoard,
	TaskItemRow,
	TaskRangeKind,
	UpdateTaskRequest,
} from "@/api/models";
import { salesKeys, tasksKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

type BoardFilter = { range: TaskRangeKind; timeZone: string; companyId?: string };

const getBoardServerFn = createServerFn({ method: "GET" })
	.validator((input: BoardFilter) => input)
	.handler(async ({ data }) => getTaskBoard(data, await getFnOptions()));

const getTaskServerFn = createServerFn({ method: "GET" })
	.validator((input: string) => input)
	.handler(async ({ data }) => getTask(data, await getFnOptions()));

const createTaskServerFn = createServerFn({ method: "POST" })
	.validator((input: CreateTaskRequest) => input)
	.handler(async ({ data }) => createTask(data, await getFnOptions()));

const updateTaskServerFn = createServerFn({ method: "POST" })
	.validator((input: { id: string; req: UpdateTaskRequest }) => input)
	.handler(async ({ data }) => updateTask(data.id, data.req, await getFnOptions()));

const completeTaskServerFn = createServerFn({ method: "POST" })
	.validator((input: string) => input)
	.handler(async ({ data }) => completeTask(data, await getFnOptions()));

const reopenTaskServerFn = createServerFn({ method: "POST" })
	.validator((input: string) => input)
	.handler(async ({ data }) => reopenTask(data, await getFnOptions()));

/** The caller's own tasks for a day, a week or a month, cut by the API in the given time zone. */
export function useTaskBoard(filter: BoardFilter) {
	return useQuery({
		queryKey: tasksKeys.board(filter),
		queryFn: () => getBoardServerFn({ data: filter }),
	});
}

export function useGetTask(id: string) {
	return useQuery({
		queryKey: tasksKeys.details(id),
		enabled: Boolean(id),
		queryFn: () => getTaskServerFn({ data: id }),
	});
}

/**
 * Moves a task between the two sections of every board in the cache. What the API will answer
 * after its projection catches up, shown now: a tick has to land on the click, not 1.5 s later.
 */
function moved(board: TaskBoard, taskId: string, done: boolean): TaskBoard {
	const from = done ? board.active : board.completed;
	const task = from.find((candidate) => candidate.id === taskId);

	if (!task) return board;

	const changed: TaskItemRow = done
		? { ...task, status: "Done", isOverdue: false, completedAt: new Date().toISOString() }
		: { ...task, status: "Open", completedAt: null, isOverdue: new Date(task.dueAt) < new Date() };

	const rest = from.filter((candidate) => candidate.id !== taskId);

	return done
		? { ...board, active: rest, completed: [changed, ...board.completed] }
		: {
				...board,
				completed: rest,
				active: [...board.active, changed].sort((a, b) => a.dueAt.localeCompare(b.dueAt)),
			};
}

/**
 * Done and undone with one click. Optimistic, rolled back when the API refuses, and read again
 * once the projection has caught up - also the opportunity's history, which gets a "Task" entry.
 */
export function useToggleTask() {
	const client = useQueryClient();
	const { wait } = useProjectionWait();

	return useMutation<
		unknown,
		Error,
		{ task: TaskItemRow; done: boolean },
		{ previous: [QueryKey, TaskBoard | undefined][] }
	>({
		mutationFn: ({ task, done }) =>
			done ? completeTaskServerFn({ data: task.id }) : reopenTaskServerFn({ data: task.id }),

		onMutate: async ({ task, done }) => {
			await client.cancelQueries({ queryKey: tasksKeys.boards() });

			const previous = client.getQueriesData<TaskBoard>({ queryKey: tasksKeys.boards() });

			client.setQueriesData<TaskBoard>({ queryKey: tasksKeys.boards() }, (board) =>
				board ? moved(board, task.id, done) : board,
			);

			return { previous };
		},

		onError: (_error, _variables, context) => {
			for (const [key, board] of context?.previous ?? []) client.setQueryData(key, board);
		},

		onSettled: async (_data, _error, { task }) => {
			await wait();
			await client.invalidateQueries({ queryKey: tasksKeys.all });

			if (task.opportunity) {
				await client.invalidateQueries({ queryKey: salesKeys.all });
			}
		},
	});
}

type SaveOptions = { onSuccess: () => void };

type SaveVariables =
	| { mode: "create"; request: CreateTaskRequest }
	| { mode: "edit"; id: string; request: UpdateTaskRequest };

/** One mutation for the drawer, which creates and edits with the same fields. */
export function useSaveTask({ onSuccess }: SaveOptions) {
	const client = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation<unknown, Error, SaveVariables>({
		mutationFn: (variables) =>
			variables.mode === "create"
				? createTaskServerFn({ data: variables.request })
				: updateTaskServerFn({ data: { id: variables.id, req: variables.request } }),

		onSuccess: async () => {
			await wait();
			await client.invalidateQueries({ queryKey: tasksKeys.all });
			onSuccess();
		},
	});

	return { mutation, waiting };
}
