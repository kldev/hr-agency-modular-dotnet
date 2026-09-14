import { createFileRoute } from "@tanstack/react-router";
import InterviewCalendarPage from "#/features/calendar/pages/InterviewCalendarPage";

export const Route = createFileRoute("/app/calendar/")({
	component: RouteComponent,
	staticData: {
		breadcrumb: "Calendar",
	},
});

function RouteComponent() {
	return <InterviewCalendarPage />;
}
