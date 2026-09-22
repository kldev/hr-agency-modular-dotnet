import { Link } from "@tanstack/react-router";
import { PanelMockup } from "./PanelMockup";

export function Hero() {
	return (
		<section id="top" className="landing-hero">
			<div className="landing-hero-inner">
				<div className="landing-hero-copy">
					<h1>Run your agency from the first call to the last time sheet.</h1>

					<p>
						Sales, recruitment, the people you place with clients and your own team's hours, in one
						workspace built for IT recruitment agencies.
					</p>

					<div className="landing-hero-actions">
						<a href="#contact" className="landing-cta">
							Book a demo
						</a>
						<Link to="/login" className="landing-cta-quiet">
							Sign in to your agency
						</Link>
					</div>
				</div>

				<PanelMockup />
			</div>
		</section>
	);
}
