import { useEffect } from "react";
import { useUiStore } from "@/stores/uiStore";
import { BoardAndFeeds } from "./components/BoardAndFeeds";
import { Compliance } from "./components/Compliance";
import { ContactSection } from "./components/ContactSection";
import { Faq } from "./components/Faq";
import { Hero } from "./components/Hero";
import { HowItWorks } from "./components/HowItWorks";
import { LandingFooter } from "./components/LandingFooter";
import { LandingNav } from "./components/LandingNav";
import { Pillars } from "./components/Pillars";
import "./landing.css";

export default function LandingPage() {
	const { mode } = useUiStore();

	// Same as `AppLayout`: the store only remembers the choice, the document has to be told.
	useEffect(() => {
		document.documentElement.dataset.theme = mode;
	}, [mode]);

	return (
		<div className="landing">
			<LandingNav />
			<main>
				<Hero />
				<Pillars />
				<BoardAndFeeds />
				<Compliance />
				<HowItWorks />
				<Faq />
				<ContactSection />
			</main>
			<LandingFooter />
		</div>
	);
}
