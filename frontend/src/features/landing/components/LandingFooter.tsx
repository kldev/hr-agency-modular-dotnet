import { Link } from "@tanstack/react-router";
import { BriefcaseBusiness } from "lucide-react";
import { navLinks } from "../content";

export function LandingFooter() {
	return (
		<footer className="landing-footer">
			<div className="landing-container landing-footer-inner">
				<div className="landing-brand">
					<span className="landing-brand-mark">
						<BriefcaseBusiness size={16} strokeWidth={2.2} />
					</span>
					HR Agency Portal
				</div>

				<nav aria-label="Footer">
					{navLinks.map((link) => (
						<a key={link.href} href={link.href}>
							{link.label}
						</a>
					))}
					<Link to="/login">Sign in</Link>
				</nav>

				<small>© {new Date().getFullYear()} HR Agency Portal</small>
			</div>
		</footer>
	);
}
