import type { TaskPriority } from "#/api/models";

export const taskPriorities: Record<TaskPriority, string> = {
	Low: "Low",
	Medium: "Medium",
	High: "High",
};

export const taskPriorityClass: Record<TaskPriority, string> = {
	Low: "badge-inactive",
	Medium: "badge-proposal",
	High: "badge-lost",
};
