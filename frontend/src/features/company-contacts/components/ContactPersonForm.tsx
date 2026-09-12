import type { ContactPerson } from "@/api/models";
import { Input } from "@/components/ui";

export type ContactPersonFormMode = "company-create-form" | "contact-form";

interface ContactPersonFormProps {
	value: ContactPerson;
	onChange: (value: ContactPerson) => void;
	mode: ContactPersonFormMode;
	disabled?: boolean;
}

export function ContactPersonForm({
	value,
	onChange,
	mode,
	disabled = false,
}: ContactPersonFormProps) {
	const required = mode === "contact-form";

	const update = <K extends keyof ContactPerson>(field: K, fieldValue: ContactPerson[K]) => {
		onChange({
			...value,
			[field]: fieldValue,
		});
	};

	return (
		<div className="drawer-form">
			<div className="form-field">
				<label className="form-label" htmlFor="contact-first-name">
					First name{required && " *"}
				</label>

				<Input
					id="contact-first-name"
					value={value.firstName}
					disabled={disabled}
					onChange={(event) => update("firstName", event.target.value)}
				/>
			</div>

			<div className="form-field">
				<label className="form-label" htmlFor="contact-last-name">
					Last name{required && " *"}
				</label>

				<Input
					id="contact-last-name"
					value={value.lastName}
					disabled={disabled}
					onChange={(event) => update("lastName", event.target.value)}
				/>
			</div>

			<div className="form-field">
				<label className="form-label" htmlFor="contact-email">
					Email{required && " *"}
				</label>

				<Input
					id="contact-email"
					type="email"
					value={value.email}
					disabled={disabled}
					onChange={(event) => update("email", event.target.value)}
				/>
			</div>

			<div className="form-field">
				<label className="form-label" htmlFor="contact-job-title">
					Job title
				</label>

				<Input
					id="contact-job-title"
					value={value.jobTitle}
					disabled={disabled}
					onChange={(event) => update("jobTitle", event.target.value)}
				/>
			</div>

			<div className="form-field">
				<label className="form-label" htmlFor="contact-phone">
					Phone
				</label>

				<Input
					id="contact-phone"
					type="tel"
					value={value.phone}
					disabled={disabled}
					onChange={(event) => update("phone", event.target.value)}
				/>
			</div>
		</div>
	);
}
