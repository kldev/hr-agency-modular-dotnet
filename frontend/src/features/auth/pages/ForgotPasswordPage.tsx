import { ArrowLeft, ArrowRight, CheckCircle2, Mail, ShieldCheck } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { AuthLayout } from "../layout";

const ForgotPasswordPage: React.FC = () => {
	const [email, setEmail] = useState("");
	const [isLoading, setIsLoading] = useState(false);
	const [submitted, setSubmitted] = useState(false);
	const [error, setError] = useState("");

	async function handleSubmit(event: React.SubmitEvent<HTMLFormElement>) {
		event.preventDefault();

		setError("");

		if (!email.trim()) {
			setError("Enter your email address.");
			return;
		}

		setIsLoading(true);

		try {
			// Replace with real password reset API.
			await new Promise((resolve) => setTimeout(resolve, 700));

			setSubmitted(true);
		} catch {
			setError("Unable to process your request. Please try again.");
		} finally {
			setIsLoading(false);
		}
	}

	if (submitted) {
		return (
			<AuthLayout>
				<div className="auth-form-header">
					<div className="auth-form-icon auth-form-icon-success">
						<CheckCircle2 size={20} />
					</div>

					<h2>Check your inbox</h2>

					<p>
						If an account exists for <strong>{email}</strong>, we have sent instructions to reset
						your password.
					</p>
				</div>

				<div className="auth-success-box">
					<div className="auth-success-icon">
						<Mail size={18} />
					</div>

					<div>
						<strong>Password reset email sent</strong>

						<p>
							Check your inbox and follow the link in the email. The link may expire for security
							reasons.
						</p>
					</div>
				</div>

				<Link to="/login" className="button button-primary auth-submit">
					<ArrowLeft size={16} />
					Back to sign in
				</Link>

				<div className="auth-security-note">
					<ShieldCheck size={14} />

					<span>For security, we don't reveal whether an account exists for this email.</span>
				</div>
			</AuthLayout>
		);
	}

	return (
		<AuthLayout>
			<div className="auth-form-header">
				<div className="auth-form-icon">
					<Mail size={19} />
				</div>

				<h2>Forgot your password?</h2>

				<p>
					Enter the email address associated with your account and we'll send you a password reset
					link.
				</p>
			</div>

			<form className="auth-form" onSubmit={handleSubmit}>
				{error && (
					<div className="auth-alert auth-alert-danger" role="alert">
						{error}
					</div>
				)}

				<div className="auth-field">
					<label htmlFor="reset-email">Email address</label>

					<div className="auth-input-wrapper">
						<Mail size={17} />

						<input
							id="reset-email"
							name="email"
							type="email"
							autoComplete="email"
							placeholder="you@company.com"
							value={email}
							onChange={(event) => setEmail(event.target.value)}
							disabled={isLoading}
						/>
					</div>

					<span className="auth-field-hint">
						We'll send the reset instructions to this address.
					</span>
				</div>

				<button type="submit" className="button button-primary auth-submit" disabled={isLoading}>
					{isLoading ? (
						<>
							<span className="spinner" />
							Sending...
						</>
					) : (
						<>
							Send reset link
							<ArrowRight size={16} />
						</>
					)}
				</button>
			</form>

			<Link to="/login" className="auth-back-link">
				<ArrowLeft size={15} />
				Back to sign in
			</Link>
		</AuthLayout>
	);
};

export default ForgotPasswordPage;
