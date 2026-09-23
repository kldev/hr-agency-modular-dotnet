import { createFileRoute } from "@tanstack/react-router";
import type { CandidateSource, JobApplicationStatus } from "#/api/models";
import ApplicationDetailsPage, {
	type ApplicationTab,
	applicationTabs,
} from "#/features/applications/pages/ApplicationDetailsPage";

export const Route = createFileRoute("/app/applications/$id")({
	component: RouteComponent,
	validateSearch: (search) => ({
		status: typeof search.status === "string" ? (search.status as JobApplicationStatus) : undefined,
		source: typeof search.source === "string" ? (search.source as CandidateSource) : undefined,

		search: typeof search.search === "string" ? search.search : undefined,

		tab: applicationTabs.includes(search.tab as ApplicationTab)
			? (search.tab as ApplicationTab)
			: undefined,
	}),
});

function RouteComponent() {
	const { id } = Route.useParams();
	const { tab } = Route.useSearch();
	const navigate = Route.useNavigate();

	return (
		<ApplicationDetailsPage
			id={id}
			tab={tab ?? "details"}
			onTabChange={(next) => navigate({ search: (previous) => ({ ...previous, tab: next }) })}
		/>
	);
}
