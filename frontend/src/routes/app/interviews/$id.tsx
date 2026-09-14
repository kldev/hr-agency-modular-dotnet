import { createFileRoute } from "@tanstack/react-router";
import type { InterviewStatus } from "#/api/models";

export const Route = createFileRoute("/app/interviews/$id")({
	component: RouteComponent,
	validateSearch: (search) => ({
		status: typeof search.status === "string" ? (search.status as InterviewStatus) : undefined,
		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	return <div>Hello "/app/interviews/$id"!</div>;
}
