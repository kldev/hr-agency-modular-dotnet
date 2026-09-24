import { createFileRoute } from "@tanstack/react-router";
import FormBuilderPage, {
	type BuilderTab,
	builderTabs,
} from "#/features/forms/pages/FormBuilderPage";

export const Route = createFileRoute("/app/forms/$formId")({
	component: RouteComponent,
	staticData: { breadcrumb: "Form" },

	/* The open tab is part of the address, so a link can point straight at the preview. */
	validateSearch: (search) => ({
		tab: builderTabs.includes(search.tab as BuilderTab) ? (search.tab as BuilderTab) : undefined,
	}),
});

function RouteComponent() {
	const { formId } = Route.useParams();
	const { tab } = Route.useSearch();
	const navigate = Route.useNavigate();

	return (
		<FormBuilderPage
			formId={formId}
			tab={tab ?? "build"}
			onTabChange={(next) => navigate({ search: (previous) => ({ ...previous, tab: next }) })}
		/>
	);
}
