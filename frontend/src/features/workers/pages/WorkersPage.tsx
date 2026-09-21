import { HardHat } from "lucide-react";
import type React from "react";
import { Page } from "@/components/layout";
import { EmptyState, LoadMore } from "@/components/ui";
import type { WorkersSearch } from "../searchParams";
import { HOME_WORK_COUNTRY } from "../types";
import { WorkersCardList, WorkersTable } from "./components/table";
import { WorkersToolbar } from "./components/WorkersToolbar";
import { useGetWorkersSlice } from "./hooks";

/**
 * `all` is the whole register; `abroad` is the desk that looks after people working outside the
 * home country.
 *
 * The two are not symmetrical and that is deliberate on the API's side: `workCountry=[PL]` keeps
 * only people who are currently on a Polish posting, which drops everybody who has no posting yet,
 * while `excludeWorkCountry=[PL]` keeps them. So the first view filters on nothing at all - a
 * register that hides the person you registered five minutes ago is not a register.
 */
export type WorkersScope = "all" | "abroad";

interface WorkersPageProps {
	scope: WorkersScope;
	search: WorkersSearch;
	/** Merged into the URL by the route that owns it; the two routes have different search types. */
	onSearchChange: (next: Partial<WorkersSearch>) => void;
	onClear: () => void;
}

const WorkersPage: React.FC<WorkersPageProps> = ({ scope, search, onSearchChange, onClear }) => {
	const query = useGetWorkersSlice({
		search: search.search,
		status: search.status ? [search.status] : undefined,
		department: search.department ? [search.department] : undefined,
		citizenship: search.citizenship,
		excludeWorkCountry: scope === "abroad" ? [HOME_WORK_COUNTRY] : undefined,
	});

	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = query.isFetched && items.length === 0;

	return (
		<Page
			className="has-mobile-view"
			title={scope === "abroad" ? "Workers abroad" : "Workers"}
			description={
				scope === "abroad"
					? "People on a posting outside the home country, and everybody not yet placed."
					: "The register of people: who they are, where they work and what still has to be in order."
			}
			onRefresh={() => query.refetch()}
			loading={query.isPending}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No people found">
					<HardHat size={24} />
				</EmptyState>
			}
		>
			<WorkersToolbar
				search={search.search ?? ""}
				status={search.status ?? null}
				department={search.department ?? null}
				onSearchChange={(value) => onSearchChange({ search: value })}
				onStatusChange={(value) => onSearchChange({ status: value ?? undefined })}
				onDepartmentChange={(value) => onSearchChange({ department: value ?? undefined })}
				onClear={onClear}
				onAdd={() => {
					// The registration wizard lands with the next step of the plan.
				}}
			/>

			<WorkersTable workers={items} />

			<WorkersCardList workers={items} />

			<LoadMore
				loading={query.isPending}
				hasNext={hasMore[0]}
				onClick={() => {
					query.fetchNextPage();
				}}
			/>
		</Page>
	);
};

export default WorkersPage;
