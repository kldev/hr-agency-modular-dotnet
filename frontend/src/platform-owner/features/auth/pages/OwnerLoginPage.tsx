import { ArrowRight, Eye, EyeOff, LockKeyhole, Mail } from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { getAuthenticatedOwner, loginPlatformOwner } from "@/api/endpoints";
import { useOwnerAuthStore } from "@/platform-owner/stores/authOwnerStore";
import { OWNER_ROUTES } from "@/routes/OwnerRoutes";

import { AuthLayout } from "../layout";

const LoginPage: React.FC = () => {
	const navigate = useNavigate();

	const [email, setEmail] = useState(import.meta.env.VITE_DEFAULT_OWNER || "");
	const [password, setPassword] = useState(import.meta.env.VITE_DEFAULT_PASSWORD || "");
	const [showPassword, setShowPassword] = useState(false);
	const [isLoading, setIsLoading] = useState(false);
	const [error, setError] = useState("");
	const store = useOwnerAuthStore();

	async function handleSubmit(event: React.SubmitEvent<HTMLFormElement>) {
		event.preventDefault();

		setError("");

		if (!email.trim()) {
			setError("Enter your email address.");
			return;
		}

		if (!password) {
			setError("Enter your password.");
			return;
		}

		setIsLoading(true);

		try {
			const result = await loginPlatformOwner({ email: email, password: password });

			store.setToken(result.token);
			if (result.token) {
				const owner = await getAuthenticatedOwner();
				store.setOwner(owner);
			}

			navigate(OWNER_ROUTES.ORGANIZATIONS);
		} catch {
			setError("Unable to sign in. Please try again.");
		} finally {
			setIsLoading(false);
		}
	}

	return (
		<AuthLayout>
			<div className="auth-form-header">
				<div className="auth-form-icon">
					<LockKeyhole size={19} />
				</div>

				<h2>Welcome back</h2>

				<p>
					Administration panel <strong>HR Agency Portal</strong>
				</p>
			</div>

			<form className="auth-form" onSubmit={handleSubmit}>
				{error && (
					<div className="auth-alert auth-alert-danger" role="alert">
						{error}
					</div>
				)}

				<div className="auth-field">
					<label htmlFor="email">Email address</label>

					<div className="auth-input-wrapper">
						<Mail size={17} />

						<input
							id="email"
							name="email"
							type="email"
							autoComplete="email"
							placeholder="you@company.com"
							value={email}
							onChange={(event) => setEmail(event.target.value)}
							disabled={isLoading}
						/>
					</div>
				</div>

				<div className="auth-field">
					<div className="auth-input-wrapper">
						<LockKeyhole size={17} />

						<input
							id="password"
							name="password"
							type={showPassword ? "text" : "password"}
							autoComplete="current-password"
							placeholder="Enter your password"
							value={password}
							onChange={(event) => setPassword(event.target.value)}
							disabled={isLoading}
						/>

						<button
							type="button"
							className="auth-password-toggle"
							onClick={() => setShowPassword((value) => !value)}
							aria-label={showPassword ? "Hide password" : "Show password"}
							disabled={isLoading}
						>
							{showPassword ? <EyeOff size={17} /> : <Eye size={17} />}
						</button>
					</div>
				</div>

				<button type="submit" className="button button-primary auth-submit" disabled={isLoading}>
					{isLoading ? (
						<>
							<span className="spinner" />
							Signing in...
						</>
					) : (
						<>
							Sign in
							<ArrowRight size={16} />
						</>
					)}
				</button>
			</form>
		</AuthLayout>
	);
};

export default LoginPage;
