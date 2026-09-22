import { createFileRoute } from "@tanstack/react-router";
import LandingPage from "#/features/landing/LandingPage";

export const Route = createFileRoute("/")({
	head: () => ({
		meta: [
			{ title: "HR Agency Portal - one workspace for IT recruitment agencies" },
			{
				name: "description",
				content:
					"Sales, recruitment, the people you place with clients and your own team's hours in one workspace. Leave your details and our sales team will contact you.",
			},
		],
		links: [
			{ rel: "preconnect", href: "https://fonts.googleapis.com" },
			{ rel: "preconnect", href: "https://fonts.gstatic.com", crossOrigin: "anonymous" },
			{
				rel: "stylesheet",
				href: "https://fonts.googleapis.com/css2?family=Schibsted+Grotesk:wght@400;500;600;700;800&display=swap",
			},
		],
	}),
	component: LandingPage,
});
