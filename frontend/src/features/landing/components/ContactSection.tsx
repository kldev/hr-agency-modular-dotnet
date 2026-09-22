import { CheckCircle2, Info } from "lucide-react";
import type { ReactNode } from "react";
import { Button } from "@/components/ui/Button";
import { teamSizes } from "../content";
import { useContactForm } from "./useContactForm";

interface FieldProps {
	id: string;
	label: string;
	hint?: string;
	error?: string;
	children: ReactNode;
}

function Field({ id, label, hint, error, children }: FieldProps) {
	return (
		<div className="landing-field">
			<label htmlFor={id}>
				{label}
				{hint && <span className="landing-field-hint">{hint}</span>}
			</label>
			{children}
			{error && (
				<p id={`${id}-error`} className="landing-field-error">
					{error}
				</p>
			)}
		</div>
	);
}

export function ContactSection() {
	const form = useContactForm();
	const { values, errors } = form;

	async function handleSubmit(event: React.SubmitEvent<HTMLFormElement>) {
		event.preventDefault();
		// Read before the await: React clears `currentTarget` once the handler has returned.
		const element = event.currentTarget;
		const ok = await form.submit();
		if (!ok) {
			// Send the keyboard to the first thing that needs fixing instead of leaving it on the button,
			// once the errors have rendered.
			requestAnimationFrame(() =>
				element.querySelector<HTMLElement>("[aria-invalid='true']")?.focus(),
			);
		}
	}

	const described = (field: keyof typeof errors) =>
		errors[field] ? { "aria-invalid": true, "aria-describedby": `contact-${field}-error` } : {};

	return (
		<section id="contact" className="landing-section landing-contact">
			<div className="landing-container landing-split">
				<div className="landing-split-copy">
					<h2>Interested? Leave your details and we will call you back</h2>
					<p>
						Tell us who you are and how to reach you. Someone from our sales team will get in touch,
						walk you through the panel and set up your agency when you are ready.
					</p>

					<ul className="landing-contact-list">
						<li>A walkthrough of the panel with your own use cases.</li>
						<li>Your organization, job board address and first admin account.</li>
						<li>Answers on posting people to Germany and Belgium.</li>
					</ul>
				</div>

				<div className="landing-form-card" aria-live="polite">
					{form.sent ? (
						<div className="landing-form-sent">
							<span className="landing-form-sent-icon">
								<CheckCircle2 size={22} />
							</span>
							<h3>Thank you, {form.sent.fullName.trim().split(" ")[0]}</h3>
							<p className="landing-form-sent-text">
								Our sales team will contact you{" "}
								{form.sent.email.trim() ? (
									<>
										at <strong>{form.sent.email.trim()}</strong>
									</>
								) : (
									<>
										on <strong>{form.sent.phone.trim()}</strong>
									</>
								)}{" "}
								about {form.sent.agencyName.trim()}.
							</p>
							<Button variant="secondary" onClick={form.reset}>
								Send another request
							</Button>
						</div>
					) : (
						<form className="landing-form" noValidate onSubmit={handleSubmit}>
							<div className="landing-form-row">
								<Field id="contact-fullName" label="Your name" error={errors.fullName}>
									<input
										id="contact-fullName"
										autoComplete="name"
										value={values.fullName}
										onChange={(event) => form.setField("fullName", event.target.value)}
										{...described("fullName")}
									/>
								</Field>

								<Field id="contact-agencyName" label="Agency" error={errors.agencyName}>
									<input
										id="contact-agencyName"
										autoComplete="organization"
										value={values.agencyName}
										onChange={(event) => form.setField("agencyName", event.target.value)}
										{...described("agencyName")}
									/>
								</Field>
							</div>

							<p className="landing-form-note">E-mail or phone, whichever you prefer.</p>

							<div className="landing-form-row">
								<Field id="contact-email" label="E-mail" error={errors.email}>
									<input
										id="contact-email"
										type="email"
										autoComplete="email"
										value={values.email}
										onChange={(event) => form.setField("email", event.target.value)}
										{...described("email")}
									/>
								</Field>

								<Field id="contact-phone" label="Phone" error={errors.phone}>
									<input
										id="contact-phone"
										type="tel"
										autoComplete="tel"
										value={values.phone}
										onChange={(event) => form.setField("phone", event.target.value)}
										{...described("phone")}
									/>
								</Field>
							</div>

							<Field id="contact-teamSize" label="Recruiters on your team" hint="Optional">
								<select
									id="contact-teamSize"
									value={values.teamSize}
									onChange={(event) => form.setField("teamSize", event.target.value)}
								>
									<option value="">Choose</option>
									{teamSizes.map((size) => (
										<option key={size} value={size}>
											{size}
										</option>
									))}
								</select>
							</Field>

							<Field id="contact-message" label="Anything we should know" hint="Optional">
								<textarea
									id="contact-message"
									rows={3}
									value={values.message}
									onChange={(event) => form.setField("message", event.target.value)}
								/>
							</Field>

							<Button
								type="submit"
								variant="primary"
								loading={form.sending}
								className="landing-form-submit"
							>
								Request a call back
							</Button>

							<p className="landing-demo-note">
								<Info size={14} className="landing-demo-note-icon" aria-hidden="true" />
								This is a demo. Nothing you type here is saved or sent anywhere.
							</p>
						</form>
					)}
				</div>
			</div>
		</section>
	);
}
