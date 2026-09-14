import { createFileRoute } from "@tanstack/react-router";
import UsersPage from "#/features/users/pages/UsersPage";

export const Route = createFileRoute("/app/users/")({
	component: RouteComponent,
});

function RouteComponent() {
	return <UsersPage />;
}
