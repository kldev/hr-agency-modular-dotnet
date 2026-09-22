import { createFileRoute } from "@tanstack/react-router";
import { EmploymentPage } from "#/features/agency-employment/pages/EmploymentPage";

export const Route = createFileRoute("/app/employment")({
	component: RouteComponent,
	staticData: { breadcrumb: "Employment" },

	/* Both filters live in the address so a link can point at "everybody who owes hours". */
	validateSearch: (search) => ({
		search: typeof search.search === "string" && search.search ? search.search : undefined,
		coveredOnly: search.coveredOnly === true ? true : undefined,
	}),
});

function RouteComponent() {
	return <EmploymentPage />;
}
