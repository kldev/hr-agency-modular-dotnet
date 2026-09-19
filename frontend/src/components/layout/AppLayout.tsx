import { Outlet } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import type { AppUserAuthenticated } from "#/api/models";
import { useAuthStore } from "#/stores/authStore";
import { useUiStore } from "@/stores/uiStore";
import { Sidebar } from "./sidebar";
import { TopBar } from "./top-bar";

interface AppLayoutProps {
	user: AppUserAuthenticated;
}

export function AppLayout({ user }: AppLayoutProps) {
	const [collapsed, setCollapsed] = useState(false);
	const [mobileOpen, setMobileOpen] = useState(false);
	const { mode } = useUiStore();
	const { setUser } = useAuthStore();

	useEffect(() => {
		document.documentElement.dataset.theme = mode;
	}, [mode]);

	// The route guard already resolved the user; this only mirrors it into the store the rest of
	// the tree reads from.
	useEffect(() => {
		setUser(user);
	}, [user, setUser]);

	return (
		<div
			className={[
				"app-layout",
				collapsed ? "sidebar-collapsed" : "",
				mobileOpen ? "sidebar-mobile-open" : "",
			]
				.filter(Boolean)
				.join(" ")}
		>
			<Sidebar
				mode="organization"
				collapsed={collapsed}
				mobileOpen={mobileOpen}
				onToggle={() => setCollapsed((value) => !value)}
			/>

			{mobileOpen && (
				<button
					type="button"
					className="sidebar-mobile-overlay"
					aria-label="Close navigation"
					onClick={() => setMobileOpen(false)}
				/>
			)}

			<main className="app-main">
				<TopBar onMenuClick={() => setMobileOpen((value) => !value)} />
				<Outlet />
			</main>
		</div>
	);
}
