/**
 * The workspace's state that belongs in the address bar: which company, which tab, which period of
 * tasks. A link to the screen then reopens it exactly as it was, and a refresh loses nothing.
 */
export type WorkspaceTab = "activities" | "opportunities" | "projects";

export type TaskRangeParam = "day" | "week" | "month";

export const workspaceTabs: Record<WorkspaceTab, string> = {
	activities: "Activities",
	opportunities: "Opportunities",
	projects: "Projects",
};

export const taskRanges: Record<TaskRangeParam, string> = {
	day: "Day",
	week: "Week",
	month: "Month",
};

export const defaultWorkspaceTab: WorkspaceTab = "activities";

export const defaultTaskRange: TaskRangeParam = "week";

export interface WorkspaceSearch {
	companyId?: string;
	tab?: WorkspaceTab;
	range?: TaskRangeParam;
}

/** Unknown values fall back to the default rather than breaking the page - the url is user input. */
export function parseWorkspaceSearch(search: Record<string, unknown>): WorkspaceSearch {
	const { companyId, tab, range } = search;

	return {
		companyId: typeof companyId === "string" && companyId.length > 0 ? companyId : undefined,
		tab: typeof tab === "string" && tab in workspaceTabs ? (tab as WorkspaceTab) : undefined,
		range: typeof range === "string" && range in taskRanges ? (range as TaskRangeParam) : undefined,
	};
}

/** The API names the range as its enum does. */
export function toApiRange(range: TaskRangeParam): "Day" | "Week" | "Month" {
	return taskRanges[range] as "Day" | "Week" | "Month";
}
