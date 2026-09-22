import { useState } from "react";

export interface ContactValues {
	fullName: string;
	agencyName: string;
	email: string;
	phone: string;
	teamSize: string;
	message: string;
}

export type ContactErrors = Partial<Record<"fullName" | "agencyName" | "email" | "phone", string>>;

const EMPTY: ContactValues = {
	fullName: "",
	agencyName: "",
	email: "",
	phone: "",
	teamSize: "",
	message: "",
};

export const CONTACT_MESSAGES = {
	fullNameRequired: "Enter your name.",
	agencyNameRequired: "Enter the name of your agency.",
	contactRequired: "Enter an e-mail address or a phone number, so we can reach you.",
	emailInvalid: "Enter an e-mail address like name@agency.com.",
	phoneInvalid: "Enter a phone number with at least 7 digits.",
} as const;

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_PATTERN = /^\+?[\d\s()-]+$/;

export function validateContact(values: ContactValues): ContactErrors {
	const errors: ContactErrors = {};
	const email = values.email.trim();
	const phone = values.phone.trim();

	if (!values.fullName.trim()) errors.fullName = CONTACT_MESSAGES.fullNameRequired;
	if (!values.agencyName.trim()) errors.agencyName = CONTACT_MESSAGES.agencyNameRequired;

	// Either channel will do, but one we cannot use is worse than none: it reads as "we will call"
	// and then nobody can.
	if (!email && !phone) {
		errors.email = CONTACT_MESSAGES.contactRequired;
	}
	if (email && !EMAIL_PATTERN.test(email)) errors.email = CONTACT_MESSAGES.emailInvalid;
	if (phone && (!PHONE_PATTERN.test(phone) || phone.replace(/\D/g, "").length < 7)) {
		errors.phone = CONTACT_MESSAGES.phoneInvalid;
	}

	return errors;
}

/**
 * The contact form of the landing page. It is a demonstration: nothing leaves the browser, and
 * "sent" only means the values passed validation. There is no leads endpoint behind it yet.
 */
export function useContactForm() {
	const [values, setValues] = useState<ContactValues>(EMPTY);
	const [errors, setErrors] = useState<ContactErrors>({});
	const [sending, setSending] = useState(false);
	const [sent, setSent] = useState<ContactValues | null>(null);

	function setField<K extends keyof ContactValues>(field: K, value: ContactValues[K]) {
		setValues((current) => ({ ...current, [field]: value }));
		// Typing into either channel settles "give us one of the two", so both lose their error.
		setErrors((current) => {
			if (field === "email" || field === "phone") {
				const { email: _email, phone: _phone, ...rest } = current;
				return rest;
			}
			const { [field as keyof ContactErrors]: _removed, ...rest } = current;
			return rest;
		});
	}

	async function submit() {
		const found = validateContact(values);
		setErrors(found);
		if (Object.keys(found).length > 0) return false;

		setSending(true);
		// Long enough for the button to show it is working rather than flicker.
		await new Promise((resolve) => setTimeout(resolve, 600));
		setSending(false);
		setSent(values);
		return true;
	}

	function reset() {
		setValues(EMPTY);
		setErrors({});
		setSent(null);
	}

	return { values, errors, sending, sent, setField, submit, reset };
}
