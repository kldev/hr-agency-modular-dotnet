import { Label, TimeField } from "@heroui/react";
import type { Time } from "@internationalized/date";
import { Clock } from "lucide-react";
import type React from "react";
import { useEffect, useState } from "react";
import { I18nProvider } from "react-aria-components";
import {
	DEFAULT_FROM_MINUTES,
	DEFAULT_TO_MINUTES,
	formatTimeOfDay,
	parseTimeOfDay,
	timeError,
} from "./timeInputUtils";

interface TimeInputProps {
	value?: string;
	onChange: (value: string) => void;
	onBlur?: () => void;
	disabled?: boolean;
	error?: string;
	id?: string;
	name?: string;

	/** A visible label inside the field, which names it for assistive technology too. */
	label?: React.ReactNode;

	"aria-label"?: string;

	/**
	 * The window a time may fall in, in minutes since midnight. The default is an office day,
	 * which is what every caller but one wants; a shift that starts at eleven at night is a legal
	 * work day (it simply ends on the next one), so the register has to be able to say so.
	 */
	fromMinutes?: number;
	toMinutes?: number;
}

/** "hh:mm" in an empty field, in place of React Aria's dashes - the shape to type in. */
const TIME_PLACEHOLDERS: Partial<Record<string, string>> = { hour: "hh", minute: "mm" };

/**
 * HeroUI's `TimeField`: hours and minutes typed as segments, 24-hour, "HH:mm" in and out.
 *
 * It replaced a list of every fifth minute between the limits, so it keeps what the list
 * guaranteed: a time outside `fromMinutes`..`toMinutes`, or between two five-minute steps, stays
 * in the field with the reason under it and never reaches the form.
 */
export function TimeInput({
	value = "",
	onChange,
	onBlur,
	disabled = false,
	error,
	id,
	name,
	label,
	"aria-label": ariaLabel,
	fromMinutes = DEFAULT_FROM_MINUTES,
	toMinutes = DEFAULT_TO_MINUTES,
}: TimeInputProps) {
	// Replaced only when the time really changes - React Aria resets the segments on a new object.
	const [fieldValue, setFieldValue] = useState<Time | null>(() => parseTimeOfDay(value));
	const [typedError, setTypedError] = useState<string | null>(null);

	useEffect(() => {
		setFieldValue((current) =>
			(current ? formatTimeOfDay(current) : "") === value ? current : parseTimeOfDay(value),
		);
		setTypedError(null);
	}, [value]);

	/*
	 * A refusal is said once the field is left. Minutes are typed digit by digit and the field
	 * reports 08:03 on the way to 08:30, so judging every keystroke would flash "steps of 5" at
	 * everybody typing a perfectly good time.
	 */
	const [focused, setFocused] = useState(false);

	const accept = (next: Time | null) => {
		setFieldValue(next);

		if (!next) {
			setTypedError(null);
			if (value) onChange("");
			return;
		}

		const refused = timeError(next, fromMinutes, toMinutes);

		setTypedError(refused);

		if (!refused && formatTimeOfDay(next) !== value) {
			onChange(formatTimeOfDay(next));
		}
	};

	const message = (focused ? null : typedError) ?? error;

	return (
		<I18nProvider locale="pl">
			<TimeField
				fullWidth
				className="panel-time-field"
				id={id}
				name={name}
				aria-label={label ? undefined : (ariaLabel ?? "Time")}
				value={fieldValue}
				onChange={(next) => accept(next as Time | null)}
				onFocusChange={setFocused}
				onBlur={onBlur}
				// The window is checked by `timeError`, not by React Aria: its minValue/maxValue would
				// turn the field red at the first digit of the hour.
				isDisabled={disabled}
				isInvalid={Boolean(message)}
				validationBehavior="aria"
				hourCycle={24}
				granularity="minute"
				shouldForceLeadingZeros
			>
				{label && <Label className="form-label">{label}</Label>}

				<TimeField.Group fullWidth className="panel-date-field">
					<TimeField.Prefix>
						<Clock size={16} strokeWidth={1.8} className="panel-date-field__icon" />
					</TimeField.Prefix>

					<TimeField.Input>
						{(segment) => (
							<TimeField.Segment segment={segment}>
								{({ isPlaceholder, text, type }) =>
									isPlaceholder ? (TIME_PLACEHOLDERS[type] ?? text) : text
								}
							</TimeField.Segment>
						)}
					</TimeField.Input>
				</TimeField.Group>

				{message && <div className="panel-date-field__error">{message}</div>}
			</TimeField>
		</I18nProvider>
	);
}

export default TimeInput;
