import { ChevronDown, Menu } from "lucide-react";
import { useCallback, useState } from "react";
import UserMenu from "./UserMenu";
import "./topbar.css";
import { avatarUrl, useGetOwnProfile } from "#/features/profile/pages/hooks";
import { useAuthStore } from "@/stores/authStore";

interface TopBarProps {
	onMenuClick: () => void;
}

export function TopBar({ onMenuClick }: TopBarProps) {
	const [open, setOpen] = useState(false);
	const close = useCallback(() => setOpen(false), []);
	const { user } = useAuthStore();

	/*
	 * The store holds the token's own account of who this is, which is what the route guard needs.
	 * It is not what should be displayed: the name in a token is as old as the token, so editing your
	 * own profile would leave the wrong name up here until the next sign-in. The profile endpoint
	 * answers the display question, and every profile mutation invalidates it.
	 */
	const profile = useGetOwnProfile();

	const fullName = profile.data?.user.fullName ?? user?.fullName ?? "";
	const picture = avatarUrl(profile.data?.avatarFileId);

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
						{picture ? (
							<img src={picture} alt="" className="user-avatar" />
						) : (
							<span className="user-avatar">{fullName.charAt(0)}</span>
						)}
						<span className="user-info">
							<span className="user-name">{fullName}</span>
							<span className="user-role">{user?.role}</span>
						</span>
						<ChevronDown size={15} />
					</button>

					{open && <UserMenu onClose={close} />}
				</div>
			</div>
		</header>
	);
}
