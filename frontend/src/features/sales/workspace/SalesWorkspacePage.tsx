import { useQueryClient } from "@tanstack/react-query";
import { Building2 } from "lucide-react";
import { useEffect, useRef } from "react";
import Page from "#/components/layout/page/Page";
import { EmptyState, TabPanel, Tabs } from "#/components/ui";
import { useGetCompany } from "#/features/companies/pages/hooks";
import { TasksPanel } from "#/features/tasks/components/TasksPanel";
import { TaskDrawer, type TaskDrawerRef } from "#/features/tasks/drawers/TaskDrawer";
import { salesKeys } from "@/api/query-keys";
import { ActivitiesTab } from "./components/ActivitiesTab";
import { CompanyPanel } from "./components/CompanyPanel";
import { CompanySwitcher } from "./components/CompanySwitcher";
import {
	LogCompanyActivityDrawer,
	type LogCompanyActivityRef,
} from "./components/LogCompanyActivityDrawer";
import { OpportunitiesTab } from "./components/OpportunitiesTab";
import { ProjectsTab } from "./components/ProjectsTab";
import {
	defaultTaskRange,
	defaultWorkspaceTab,
	type TaskRangeParam,
	type WorkspaceSearch,
	type WorkspaceTab,
	workspaceTabs,
} from "./search";
import "./sales-workspace.css";

const LAST_COMPANY_KEY = "sales-workspace.company";

/** The last company only as a convenience: the address is what says which one is on screen. */
function rememberedCompany(): string | undefined {
	try {
		return window.localStorage.getItem(LAST_COMPANY_KEY) ?? undefined;
	} catch {
		return undefined;
	}
}

function remember(companyId: string) {
	try {
		window.localStorage.setItem(LAST_COMPANY_KEY, companyId);
	} catch {
		// private window or blocked storage - the url still carries the company
	}
}

interface SalesWorkspacePageProps {
	search: WorkspaceSearch;
	onSearchChange: (next: WorkspaceSearch, replace?: boolean) => void;
}

/**
 * A salesperson's day on one screen: the company they are working with, what happened on its
 * deals and what it bought, and their own to-do list across every company. The page owns the
 * drawers, the url owns the state - company, tab and task period.
 */
export default function SalesWorkspacePage({ search, onSearchChange }: SalesWorkspacePageProps) {
	const client = useQueryClient();
	const taskDrawer = useRef<TaskDrawerRef>(null);
	const logDrawer = useRef<LogCompanyActivityRef>(null);

	const companyId = search.companyId;
	const tab = search.tab ?? defaultWorkspaceTab;
	const range = search.range ?? defaultTaskRange;
	const company = useGetCompany(companyId ?? "");

	useEffect(() => {
		if (companyId) {
			remember(companyId);
			return;
		}

		const last = rememberedCompany();
		if (last) onSearchChange({ ...search, companyId: last }, true);
	}, [companyId, onSearchChange, search]);

	const pickCompany = (id: string) => onSearchChange({ ...search, companyId: id });
	const setTab = (next: WorkspaceTab) =>
		onSearchChange({ ...search, tab: next === defaultWorkspaceTab ? undefined : next });
	const setRange = (next: TaskRangeParam) =>
		onSearchChange({ ...search, range: next === defaultTaskRange ? undefined : next });

	const openNewTask = () =>
		taskDrawer.current?.create(
			company.data ? { id: company.data.id, name: company.data.name } : undefined,
		);

	return (
		<Page
			title="Sales workspace"
			description="The company you work with, its deals and projects, and your tasks."
			emptyState={null}
			wide
		>
			<div className="sales-workspace">
				<div className="sales-workspace-company">
					{companyId ? (
						<CompanyPanel companyId={companyId} onChange={pickCompany} />
					) : (
						<section className="data-details-section workspace-company">
							<div className="workspace-company-body">
								<EmptyState
									title="Pick a company"
									description="Its activities, opportunities and projects open here."
								>
									<Building2 size={24} />
								</EmptyState>
								<CompanySwitcher onPick={pickCompany} />
							</div>
						</section>
					)}
				</div>

				<div className="sales-workspace-main">
					<Tabs
						value={tab}
						label="Company"
						onChange={setTab}
						tabs={(Object.keys(workspaceTabs) as WorkspaceTab[]).map((id) => ({
							id,
							label: workspaceTabs[id],
						}))}
					/>

					{companyId ? (
						<TabPanel id={tab}>
							{tab === "activities" ? (
								<ActivitiesTab companyId={companyId} onLog={() => logDrawer.current?.open()} />
							) : null}
							{tab === "opportunities" ? <OpportunitiesTab companyId={companyId} /> : null}
							{tab === "projects" ? <ProjectsTab companyId={companyId} /> : null}
						</TabPanel>
					) : (
						<EmptyState
							title="No company selected"
							description="Choose a company on the left to see its activities, opportunities and projects."
						>
							<Building2 size={24} />
						</EmptyState>
					)}
				</div>

				<div className="sales-workspace-tasks">
					<TasksPanel
						range={range}
						onRangeChange={setRange}
						onAdd={openNewTask}
						onOpen={(task) => taskDrawer.current?.edit(task)}
					/>
				</div>
			</div>

			<TaskDrawer ref={taskDrawer} />

			{companyId ? (
				<LogCompanyActivityDrawer
					ref={logDrawer}
					companyId={companyId}
					onLogged={() => client.invalidateQueries({ queryKey: salesKeys.all })}
				/>
			) : null}
		</Page>
	);
}
