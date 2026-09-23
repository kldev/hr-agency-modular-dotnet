import { Link } from "@tanstack/react-router";
import clsx from "clsx";
import { BriefcaseBusiness, Menu, Moon, Sun, X } from "lucide-react";
import { useEffect, useState } from "react";
import { useUiStore } from "@/stores/uiStore";
import { navLinks } from "../content";

export function LandingNav() {
	const { mode, setMode } = useUiStore();
	const [scrolled, setScrolled] = useState(false);
	const [open, setOpen] = useState(false);
	// The store reads localStorage, which the server does not have: until hydration the icon and its
	// label would disagree with the markup the server sent, so both wait for the first client render.
	const [mounted, setMounted] = useState(false);

	useEffect(() => setMounted(true), []);

	useEffect(() => {
		const onScroll = () => setScrolled(window.scrollY > 16);
		onScroll();
		window.addEventListener("scroll", onScroll, { passive: true });
		return () => window.removeEventListener("scroll", onScroll);
	}, []);

	// Every link in the menu jumps to a section, so arriving at one closes it - the anchors stay
	// plain navigation instead of each carrying its own click handler.
	useEffect(() => {
		if (!open) return;
		const onHashChange = () => setOpen(false);
		window.addEventListener("hashchange", onHashChange);
		return () => window.removeEventListener("hashchange", onHashChange);
	}, [open]);

	useEffect(() => {
		if (!open) return;
		const onKey = (event: KeyboardEvent) => event.key === "Escape" && setOpen(false);
		window.addEventListener("keydown", onKey);
		return () => window.removeEventListener("keydown", onKey);
	}, [open]);

	const nextMode = mode === "dark" ? "light" : "dark";
	const themeLabel = mounted ? `Switch to ${nextMode} theme` : "Switch theme";

	return (
		<header className={clsx("landing-nav", (scrolled || open) && "landing-nav-solid")}>
			<div className="landing-nav-inner">
				<a href="#top" className="landing-brand">
					<span className="landing-brand-mark">
						<BriefcaseBusiness size={16} strokeWidth={2.2} />
					</span>
					HR Agency Portal
				</a>

				<nav
					id="landing-menu"
					className={clsx("landing-nav-links", open && "landing-nav-links-open")}
					aria-label="Page sections"
				>
					{navLinks.map((link) => (
						<a key={link.href} href={link.href}>
							{link.label}
						</a>
					))}

					<Link to="/login" className="landing-nav-signin-mobile">
						Sign in
					</Link>
				</nav>

				<div className="landing-nav-actions">
					<button
						type="button"
						className="landing-icon-button"
						aria-label={themeLabel}
						title={themeLabel}
						onClick={() => setMode(nextMode)}
					>
						{mounted ? mode === "dark" ? <Sun size={16} /> : <Moon size={16} /> : null}
					</button>

					<Link to="/login" className="landing-nav-signin">
						Sign in
					</Link>

					<a href="#contact" className="landing-cta landing-cta-small">
						Book a demo
					</a>

					<button
						type="button"
						className="landing-icon-button landing-menu-toggle"
						aria-label={open ? "Close menu" : "Open menu"}
						aria-expanded={open}
						aria-controls="landing-menu"
						onClick={() => setOpen((value) => !value)}
					>
						{open ? <X size={18} /> : <Menu size={18} />}
					</button>
				</div>
			</div>
		</header>
	);
}
