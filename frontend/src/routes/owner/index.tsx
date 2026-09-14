import { createFileRoute } from "@tanstack/react-router";
import OwnerLoginPage from "#/platform-owner/features/auth/pages/OwnerLoginPage";

export const Route = createFileRoute("/owner/")({
	component: RouteComponent,
});

function RouteComponent() {
	return <OwnerLoginPage />;
}
