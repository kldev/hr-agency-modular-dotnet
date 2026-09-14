import { Outlet } from "@tanstack/react-router";
import { useCallback, useEffect, useState } from "react";
import { getUserAuth } from "#/server/auth";
import { useAuthStore } from "#/stores/authStore";
import { useUiStore } from "@/stores/uiStore";
import { Sidebar } from "./sidebar";
import { TopBar } from "./top-bar";

export function AppLayout() {
	const [collapsed, setCollapsed] = useState(false);
	const [mobileOpen, setMobileOpen] = useState(false);
	const { mode } = useUiStore();
	const { setUser, clearUser } = useAuthStore();

	useEffect(() => {
		document.documentElement.dataset.theme = mode;
	}, [mode]);

	const checkTokenAndUser = useCallback(async () => {
		const user = await getUserAuth();
		if (!user) clearUser();
		setUser(user);
	}, [clearUser, setUser]);

	useEffect(() => {
		checkTokenAndUser();
	}, [checkTokenAndUser]);

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
