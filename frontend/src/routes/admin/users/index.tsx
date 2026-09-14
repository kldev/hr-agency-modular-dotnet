import { createFileRoute } from "@tanstack/react-router";
import UsersPage from "#/platform-owner/features/users/pages/UsersPage";

export const Route = createFileRoute("/admin/users/")({
	component: RouteComponent,
});

function RouteComponent() {
	return <UsersPage />;
}
