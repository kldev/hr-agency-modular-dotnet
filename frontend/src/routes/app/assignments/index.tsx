import { createFileRoute } from "@tanstack/react-router";
import AssignmentsPage from "#/features/assignments/pages/AssignmentsPage";
import {
	emptyAssignmentsSearch,
	validateAssignmentsSearch,
} from "#/features/assignments/searchParams";

export const Route = createFileRoute("/app/assignments/")({
	component: RouteComponent,
	staticData: { breadcrumb: "Assignments" },
	validateSearch: validateAssignmentsSearch,
});

function RouteComponent() {
	const search = Route.useSearch();
	const navigate = Route.useNavigate();

	return (
		<AssignmentsPage
			search={search}
			onSearchChange={(next) => navigate({ search: (previous) => ({ ...previous, ...next }) })}
			onClear={() => navigate({ search: () => emptyAssignmentsSearch })}
		/>
	);
}
