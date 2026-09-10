import { ChevronDown, Menu } from "lucide-react";
import { useState } from "react";
import UserMenu from "./UserMenu";
import "./topbar.css";
import { useAuthStore } from "@/stores/authStore";

interface TopBarProps {
	onMenuClick: () => void;
}

export function TopBar({ onMenuClick }: TopBarProps) {
	const [open, setOpen] = useState(false);
	const { user } = useAuthStore();

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
						<span className="user-avatar">{user?.fullName.charAt(0)}</span>
						<span className="user-info">
							<span className="user-name">{user?.fullName}</span>
							<span className="user-role">{user?.role}</span>
						</span>
						<ChevronDown size={15} />
					</button>

					{open && <UserMenu />}
				</div>
			</div>
		</header>
	);
}
