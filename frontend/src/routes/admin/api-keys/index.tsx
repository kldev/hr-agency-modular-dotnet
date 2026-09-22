import { createFileRoute } from "@tanstack/react-router";
import { ApiKeysPage } from "#/platform-owner/features/api-keys/pages/ApiKeysPage";

export const Route = createFileRoute("/admin/api-keys/")({
	component: RouteComponent,
});

function RouteComponent() {
	return <ApiKeysPage />;
}
