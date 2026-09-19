import { Outlet } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import type { OwnerAuthenticated } from "#/api/models";
import { OwnerTopBar } from "#/platform-owner/layout";
import { Sidebar } from "@/components/layout";
import { useUiStore } from "@/stores/uiStore";
import { useOwnerAuthStore } from "../stores/authOwnerStore";

interface OwnerLayoutProps {
	owner: OwnerAuthenticated;
}

export function OwnerLayout({ owner }: OwnerLayoutProps) {
	const [collapsed, setCollapsed] = useState(false);
	const [mobileOpen, setMobileOpen] = useState(false);
	const { mode } = useUiStore();
	const { setOwner } = useOwnerAuthStore();

	useEffect(() => {
		document.documentElement.dataset.theme = mode;
	}, [mode]);

	// The route guard already resolved the owner; this only mirrors it into the store.
	useEffect(() => {
		setOwner(owner);
	}, [owner, setOwner]);

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
