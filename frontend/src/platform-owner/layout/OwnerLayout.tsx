import { Outlet } from "@tanstack/react-router";
import { useCallback, useEffect, useState } from "react";
import { OwnerTopBar } from "#/platform-owner/layout";
import { getOwnerAuth } from "#/server/auth";
import { Sidebar } from "@/components/layout";
import { useUiStore } from "@/stores/uiStore";
import { useOwnerAuthStore } from "../stores/authOwnerStore";

export function OwnerLayout() {
	const [collapsed, setCollapsed] = useState(false);
	const [mobileOpen, setMobileOpen] = useState(false);
	const { mode } = useUiStore();
	const { setOwner, clear } = useOwnerAuthStore();

	useEffect(() => {
		document.documentElement.dataset.theme = mode;
	}, [mode]);

	const checkTokenAndUser = useCallback(async () => {
		const user = await getOwnerAuth();
		if (!user) clear();
		setOwner(user);
	}, [clear, setOwner]);

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
				mode="owner"
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
				<OwnerTopBar onMenuClick={() => setMobileOpen((value) => !value)} />
				<Outlet />
			</main>
		</div>
	);
}
