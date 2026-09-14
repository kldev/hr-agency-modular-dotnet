import clsx from "clsx";
import { ChevronLeft, ChevronRight } from "lucide-react";
import "./sidebar.css";

import { Link, useLocation } from "@tanstack/react-router";
import { menuGroups } from "./menu";
import { ownerMenu } from "./owner-menu";

interface SidebarProps {
	collapsed: boolean;
	onToggle: () => void;
	mobileOpen: boolean;
	mode: "organization" | "owner";
}

const renderOrganization = (collapsed: boolean, pathname: string) => {
	return menuGroups.map((group) => (
		<div className="sidebar-group" key={group.title || "main"}>
			{group.title && <div className="sidebar-group-title">{group.title}</div>}

			{group.items.map((item) => {
				const Icon = item.icon;
				const active = item.link === pathname;

				return (
					<Link
						to={item.link}
						key={item.link}
						href={item.link}
						className={clsx("sidebar-item", {
							active,
						})}
						aria-current={active ? "page" : undefined}
						title={collapsed ? item.label : undefined}
					>
						<span className="sidebar-item-icon">
							<Icon size={17} strokeWidth={1.8} />
						</span>
						<span className="sidebar-item-label">{item.label}</span>
					</Link>
				);
			})}
		</div>
	));
};

const renderOwner = (collapsed: boolean, pathname: string) => {
	return ownerMenu.map((group) => (
		<div className="sidebar-group" key={group.title || "main"}>
			{group.title && <div className="sidebar-group-title">{group.title}</div>}

			{group.items.map((item) => {
				const Icon = item.icon;
				const active = item.link === pathname;

				return (
					<Link
						to={item.link}
						key={item.link}
						href={item.link}
						className={clsx("sidebar-item", {
							active,
						})}
						aria-current={active ? "page" : undefined}
						title={collapsed ? item.label : undefined}
					>
						<span className="sidebar-item-icon">
							<Icon size={17} strokeWidth={1.8} />
						</span>
						<span className="sidebar-item-label">{item.label}</span>
					</Link>
				);
			})}
		</div>
	));
};

const Sidebar: React.FC<SidebarProps> = ({ collapsed, onToggle, mode }) => {
	const location = useLocation();
	return (
		<aside className="sidebar" aria-label="Main navigation">
			<div className="sidebar-header">
				<a className="sidebar-logo" href="/" aria-label="HR Agency Portal">
					<span className="sidebar-logo-mark">HR</span>
					<span className="sidebar-logo-text">Agency Portal</span>
				</a>
			</div>

			<nav className="sidebar-content">
				{mode === "organization" ? renderOrganization(collapsed, location.pathname) : null}
				{mode === "owner" ? renderOwner(collapsed, location.pathname) : null}
			</nav>

			<div className="sidebar-footer">
				<button
					type="button"
					className="sidebar-item"
					onClick={onToggle}
					aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"}
					title={collapsed ? "Expand sidebar" : "Collapse sidebar"}
				>
					<span className="sidebar-item-icon">
						{collapsed ? <ChevronRight size={17} /> : <ChevronLeft size={17} />}
					</span>
					<span className="sidebar-item-label">{collapsed ? "Expand" : "Collapse"}</span>
				</button>
			</div>
		</aside>
	);
};

export default Sidebar;
