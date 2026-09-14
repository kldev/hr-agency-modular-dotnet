import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/app/")({
	component: RouteComponent,
	beforeLoad: () => {
		throw redirect({ to: "/app/dashboard" });
	},
});

function RouteComponent() {
	return <div>Hello "/"!</div>;
}
