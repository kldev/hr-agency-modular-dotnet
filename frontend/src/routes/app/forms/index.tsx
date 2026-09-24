import { createFileRoute } from "@tanstack/react-router";
import type { FormKind } from "#/api/models";
import FormsPage from "#/features/forms/pages/FormsPage";

export const Route = createFileRoute("/app/forms/")({
	component: RouteComponent,
	staticData: { breadcrumb: "Forms" },

	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
		kind:
			search.kind === "Document" || search.kind === "Survey"
				? (search.kind as FormKind)
				: undefined,
	}),
});

function RouteComponent() {
	const search = Route.useSearch();
	const navigate = Route.useNavigate();

	return (
		<FormsPage
			search={search.search ?? ""}
			kind={search.kind ?? null}
			onSearchChange={(value) =>
				navigate({ search: (previous) => ({ ...previous, search: value || undefined }) })
			}
			onKindChange={(value) =>
				navigate({ search: (previous) => ({ ...previous, kind: value ?? undefined }) })
			}
		/>
	);
}
