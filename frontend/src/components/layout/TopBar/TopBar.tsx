import { ChevronDown, Menu } from "lucide-react";
import { useState } from "react";
import UserMenu from "./UserMenu";
import "./topbar.css";

interface TopBarProps {
	onMenuClick: () => void;
	theme: "light" | "dark";
	onThemeChange: (theme: "light" | "dark") => void;
}

export function TopBar({ onMenuClick, theme, onThemeChange }: TopBarProps) {
	const [open, setOpen] = useState(false);

	return (
		<header className="topbar">
			<div className="topbar-left">
				<button
					type="button"
					className="topbar-menu-button"
					aria-label="Open navigation"
					onClick={onMenuClick}
				>
					<Menu size={18} />
				</button>
			</div>

			<div className="topbar-right">
				<div className="user-menu">
					<button
						type="button"
						className="user-menu-trigger"
						aria-expanded={open}
						aria-haspopup="menu"
						onClick={() => setOpen((value) => !value)}
					>
						<span className="user-avatar">JS</span>
						<span className="user-info">
							<span className="user-name">John Smith</span>
							<span className="user-role">Administrator</span>
						</span>
						<ChevronDown size={15} />
					</button>

					{open && <UserMenu theme={theme} onThemeChange={onThemeChange} />}
				</div>
			</div>
		</header>
	);
}
