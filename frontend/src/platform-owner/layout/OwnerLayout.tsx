import { useEffect, useState } from "react";
import { Outlet } from "react-router-dom";
import { Sidebar } from "@/components/layout";
import OwnerAuthProvider from "@/providers/OwnerAuthProvider";
import { useUiStore } from "@/stores/uiStore";
import { OwnerTopBar } from "./owner-top-bar";

export function OwnerLayout() {
	const [collapsed, setCollapsed] = useState(false);
	const [mobileOpen, setMobileOpen] = useState(false);
	const { mode } = useUiStore();

	useEffect(() => {
		document.documentElement.dataset.theme = mode;
	}, [mode]);

	return (
		<OwnerAuthProvider>
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
		</OwnerAuthProvider>
	);
}
