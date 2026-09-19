import { createFileRoute, redirect } from "@tanstack/react-router";
import { AppLayout } from "#/components/layout/AppLayout";
import { getUserAuth } from "#/server/auth";

export const Route = createFileRoute("/app")({
	// Fallback-deny, the way the API does it: nothing under /app renders before the session is
	// confirmed. The redirect lives here because one thrown from a query never reaches the router,
	// so an expired session used to leave an empty shell on screen instead of the login page.
	beforeLoad: async () => {
		const user = await getUserAuth();

		if (!user) {
			throw redirect({ to: "/login" });
		}

		return { user };
	},
	component: RouteComponent,
});

function RouteComponent() {
	const { user } = Route.useRouteContext();

	return <AppLayout user={user} />;
}
