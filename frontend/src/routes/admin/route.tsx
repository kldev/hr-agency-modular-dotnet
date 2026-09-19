import { createFileRoute, redirect } from "@tanstack/react-router";
import { OwnerLayout } from "#/platform-owner/layout/OwnerLayout";
import { getOwnerAuth } from "#/server/auth";

export const Route = createFileRoute("/admin")({
	// Same guard as /app, against the owner endpoint - an organization user's token carries no
	// platform role, so it is refused here and lands on the owner sign-in page.
	beforeLoad: async () => {
		const owner = await getOwnerAuth();

		if (!owner) {
			throw redirect({ to: "/owner" });
		}

		return { owner };
	},
	component: RouteComponent,
});

function RouteComponent() {
	const { owner } = Route.useRouteContext();

	return <OwnerLayout owner={owner} />;
}
