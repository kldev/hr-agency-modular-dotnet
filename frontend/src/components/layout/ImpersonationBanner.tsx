import { UserCog } from "lucide-react";
import "./impersonation-banner.css";

interface ImpersonationBannerProps {
	fullName: string;
}

/**
 * Says whose account is on screen while an administrator is standing in for somebody. Worth a
 * permanent strip rather than a toast: every button on every page now acts under this person's
 * name, and the stamps on anything saved will carry theirs.
 *
 * Signing out is the way back, and the wording says so - the administrator's own session was
 * replaced rather than parked somewhere.
 */
export function ImpersonationBanner({ fullName }: ImpersonationBannerProps) {
	return (
		<div className="impersonation-banner" role="status">
			<UserCog size={16} />

			<span>
				You are working as <strong>{fullName}</strong>. Anything you do is recorded as theirs. Sign
				out to return to your own account.
			</span>
		</div>
	);
}
