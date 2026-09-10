import { useEffect, useState } from "react";
import { Outlet } from "react-router-dom";
import AuthProvider from "@/providers/AuthProvider";
import { useUiStore } from "@/stores/uiStore";
import { Sidebar } from "./sidebar";
import { TopBar } from "./top-bar";

export function AppLayout() {
	const [collapsed, setCollapsed] = useState(false);
	const [mobileOpen, setMobileOpen] = useState(false);
	const { mode } = useUiStore();

	useEffect(() => {
		document.documentElement.dataset.theme = mode;
	}, [mode]);

	return (
		<AuthProvider>
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
					collapsed={collapsed}
					mobileOpen={mobileOpen}
					onToggle={() => setCollapsed((value) => !value)}
				/>

				<main className="app-main">
					<TopBar onMenuClick={() => setMobileOpen((value) => !value)} />
					{/* View */}
					<Outlet />
				</main>
			</div>
		</AuthProvider>
	);
}
