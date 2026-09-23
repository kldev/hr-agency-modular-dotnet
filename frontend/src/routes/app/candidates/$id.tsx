import { createFileRoute } from "@tanstack/react-router";
import type { CandidateSource } from "#/api/models";
import CandidateDetailsPage, {
	type CandidateTab,
	candidateTabs,
} from "#/features/candidates/pages/CandidateDetailsPage";

export const Route = createFileRoute("/app/candidates/$id")({
	component: RouteComponent,
	validateSearch: (search) => ({
		source: typeof search.source === "string" ? (search.source as CandidateSource) : undefined,

		search: typeof search.search === "string" ? search.search : undefined,

		tab: candidateTabs.includes(search.tab as CandidateTab)
			? (search.tab as CandidateTab)
			: undefined,
	}),
});

function RouteComponent() {
	const { id } = Route.useParams();
	const { tab } = Route.useSearch();
	const navigate = Route.useNavigate();

	return (
		<CandidateDetailsPage
			id={id}
			tab={tab ?? "profile"}
			onTabChange={(next) => navigate({ search: (previous) => ({ ...previous, tab: next }) })}
		/>
	);
}
