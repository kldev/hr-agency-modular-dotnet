import { ChevronDown, Menu } from "lucide-react";
import { useState } from "react";
import OwnerMenu from "./OwnerMenu";
import "./topbar.css";
import { useOwnerAuthStore } from "@/platform-owner/stores/authOwnerStore";

interface TopBarProps {
	onMenuClick: () => void;
}

export function OwnerTopBar({ onMenuClick }: TopBarProps) {
	const [open, setOpen] = useState(false);
	const { owner } = useOwnerAuthStore();

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
						<span className="user-avatar">{owner?.email.charAt(0)}</span>
						<span className="user-info">
							<span className="user-name">{owner?.email}</span>
							<span className="user-role">{owner?.role}</span>
						</span>
						<ChevronDown size={15} />
					</button>

					{open && <OwnerMenu />}
				</div>
			</div>
		</header>
	);
}
