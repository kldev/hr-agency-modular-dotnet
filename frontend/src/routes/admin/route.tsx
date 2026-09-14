import { createFileRoute } from "@tanstack/react-router";
import { OwnerLayout } from "#/platform-owner/layout/OwnerLayout";

export const Route = createFileRoute("/admin")({
	component: RouteComponent,
});

function RouteComponent() {
	return <OwnerLayout />;
}
