import { BriefcaseBusiness } from "lucide-react";
import type { ReactNode } from "react";
import "./auth.css";

interface AuthLayoutProps {
	children: ReactNode;
}

export function AuthLayout({ children }: AuthLayoutProps) {
	return (
		<main className="auth-layout">
			<section className="auth-visual" aria-hidden="true">
				<div className="auth-visual-image" />

				<div className="auth-visual-overlay" />

				<div className="auth-visual-content">
					<div className="auth-brand auth-brand-light">
						<span className="auth-brand-mark">
							<BriefcaseBusiness size={17} strokeWidth={2.2} />
						</span>

						<span>HR Agency Platform</span>
					</div>

					<div className="auth-visual-copy">
						<div className="auth-eyebrow">
							<span className="auth-eyebrow-line" />
							Administration panel
						</div>

						<h1>HR platform</h1>

						<p>Manage organizations.</p>
					</div>
				</div>

				<div className="auth-visual-footer">© {new Date().getFullYear()} HR Agency Portal</div>
			</section>

			<section className="auth-panel">
				<div className="auth-mobile-brand auth-brand">
					<span className="auth-brand-mark">
						<BriefcaseBusiness size={17} strokeWidth={2.2} />
					</span>

					<span>HR Agency Portal</span>
				</div>

				<div className="auth-form-wrapper">{children}</div>

				<div className="auth-panel-footer">
					<span>HR Agency Portal</span>
					<span className="auth-footer-dot">•</span>
					<span>Recruitment workspace</span>
				</div>
			</section>
		</main>
	);
}
