import { useEffect, useState } from "react";
import { Sidebar } from "./Sidebar";
import { TopBar } from "./TopBar";
import { Outlet } from "react-router-dom";

interface AppLayoutProps {}

export function AppLayout({}: AppLayoutProps) {
	const [collapsed, setCollapsed] = useState(false);
	const [mobileOpen, setMobileOpen] = useState(false);
	const [theme, setTheme] = useState<"light" | "dark">("light");

	useEffect(() => {
		document.documentElement.dataset.theme = theme;
	}, [theme]);

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
				collapsed={collapsed}
				mobileOpen={mobileOpen}
				onToggle={() => setCollapsed((value) => !value)}
			/>

			<main className="app-main">
				<TopBar
					onMenuClick={() => setMobileOpen((value) => !value)}
					theme={theme}
					onThemeChange={setTheme}
				/>
				{/* View */}
				<Outlet />
			</main>
		</div>
	);
}
