import { Link, useNavigate } from "@tanstack/react-router";
import { ArrowLeft, ArrowRight, CheckCircle2, Eye, EyeOff, LockKeyhole } from "lucide-react";
import { useState } from "react";
import { completePasswordReset } from "@/api/endpoints";
import { AuthLayout } from "../layout";
import { readApiError } from "../readApiError";

type ResetPasswordPageProps = {
	id?: string;
	token?: string;
};

const ResetPasswordPage: React.FC<ResetPasswordPageProps> = ({ id, token }) => {
	const navigate = useNavigate();

	const [password, setPassword] = useState("");
	const [confirmation, setConfirmation] = useState("");
	const [showPassword, setShowPassword] = useState(false);
	const [isLoading, setIsLoading] = useState(false);
	const [submitted, setSubmitted] = useState(false);
	const [error, setError] = useState("");

	async function handleSubmit(event: React.SubmitEvent<HTMLFormElement>) {
		event.preventDefault();

		setError("");

		if (!password) {
			setError("Enter a new password.");
			return;
		}

		if (password !== confirmation) {
			setError("The two passwords do not match.");
			return;
		}

		setIsLoading(true);

		try {
			// The window itself is the backend's business: an expired link and a forged one come
			// back as the same message, and that is what gets shown.
			await completePasswordReset({ id: id ?? "", token: token ?? "", newPassword: password });

			setSubmitted(true);
		} catch (caught) {
			setError(readApiError(caught, "Unable to set the new password. Please try again."));
		} finally {
			setIsLoading(false);
		}
	}

	if (!id || !token) {
		return (
			<AuthLayout>
				<div className="auth-form-header">
					<div className="auth-form-icon">
						<LockKeyhole size={19} />
					</div>

					<h2>This link is not complete</h2>

					<p>
						Open the link exactly as it arrived in the email, or ask for a new one - reset links
						stop working after a while.
					</p>
				</div>

				<Link to="/forgot-password" className="button button-primary auth-submit">
					Request a new link
					<ArrowRight size={16} />
				</Link>

				<Link to="/login" className="auth-back-link">
					<ArrowLeft size={16} />
					Back to sign in
				</Link>
			</AuthLayout>
		);
	}

	if (submitted) {
		return (
			<AuthLayout>
				<div className="auth-form-header">
					<div className="auth-form-icon auth-form-icon-success">
						<CheckCircle2 size={20} />
					</div>

					<h2>Password changed</h2>

					<p>Your new password is active. Sign in with it to continue.</p>
				</div>

				<button
					type="button"
					className="button button-primary auth-submit"
					onClick={() => navigate({ to: "/login" })}
				>
					Go to sign in
					<ArrowRight size={16} />
				</button>
			</AuthLayout>
		);
	}

	return (
		<AuthLayout>
			<div className="auth-form-header">
				<div className="auth-form-icon">
					<LockKeyhole size={19} />
				</div>

				<h2>Set a new password</h2>

				<p>Choose the password you will use to sign in from now on.</p>
			</div>

			<form className="auth-form" onSubmit={handleSubmit}>
				{error && (
					<div className="auth-alert auth-alert-danger" role="alert">
						{error}
					</div>
				)}

				<div className="auth-field">
					<label htmlFor="new-password">New password</label>

					<div className="auth-input-wrapper">
						<LockKeyhole size={17} />

						<input
							id="new-password"
							name="newPassword"
							type={showPassword ? "text" : "password"}
							autoComplete="new-password"
							placeholder="Enter your new password"
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

				<div className="auth-field">
					<label htmlFor="confirm-password">Repeat new password</label>

					<div className="auth-input-wrapper">
						<LockKeyhole size={17} />

						<input
							id="confirm-password"
							name="confirmPassword"
							type={showPassword ? "text" : "password"}
							autoComplete="new-password"
							placeholder="Repeat your new password"
							value={confirmation}
							onChange={(event) => setConfirmation(event.target.value)}
							disabled={isLoading}
						/>
					</div>

					<span className="auth-field-hint">
						The link works only for a short while after it was requested.
					</span>
				</div>

				<button type="submit" className="button button-primary auth-submit" disabled={isLoading}>
					{isLoading ? (
						<>
							<span className="spinner" />
							Saving...
						</>
					) : (
						<>
							Set new password
							<ArrowRight size={16} />
						</>
					)}
				</button>
			</form>

			<Link to="/login" className="auth-back-link">
				<ArrowLeft size={16} />
				Back to sign in
			</Link>
		</AuthLayout>
	);
};

export default ResetPasswordPage;
