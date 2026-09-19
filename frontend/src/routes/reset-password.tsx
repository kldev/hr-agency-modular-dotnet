import { createFileRoute } from "@tanstack/react-router";
import ResetPasswordPage from "#/features/auth/pages/ResetPasswordPage";

export const Route = createFileRoute("/reset-password")({
	component: RouteComponent,
	// The mail links to /reset-password?id=...&token=...; anything else lands on the "broken link"
	// state instead of posting an empty reset.
	validateSearch: (search: Record<string, unknown>) => ({
		id: typeof search.id === "string" ? search.id : undefined,
		token: typeof search.token === "string" ? search.token : undefined,
	}),
});

function RouteComponent() {
	const { id, token } = Route.useSearch();

	return <ResetPasswordPage id={id} token={token} />;
}
