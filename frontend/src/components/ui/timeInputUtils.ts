import { Time } from "@internationalized/date";

/** The office day every caller but one wants - see `TimeInput`. */
export const DEFAULT_FROM_MINUTES = 8 * 60;
export const DEFAULT_TO_MINUTES = 22 * 60;

/** Times go in fives: the old list offered nothing else, and the backend counts in fives. */
export const TIME_STEP_MINUTES = 5;

/** "08:30" to a `Time`; anything that is not a time of day is no time at all. */
export function parseTimeOfDay(value: string | null | undefined): Time | null {
	const match = /^(\d{1,2}):(\d{2})(?::\d{2})?$/.exec(value?.trim() ?? "");

	if (!match) {
		return null;
	}

	const hours = Number(match[1]);
	const minutes = Number(match[2]);

	return hours < 24 && minutes < 60 ? new Time(hours, minutes) : null;
}

/** A `Time` back to the "HH:mm" the forms keep. */
export function formatTimeOfDay(time: { hour: number; minute: number }): string {
	return `${String(time.hour).padStart(2, "0")}:${String(time.minute).padStart(2, "0")}`;
}

export function minutesToTime(minutes: number): Time {
	return new Time(Math.floor(minutes / 60), minutes % 60);
}

/**
 * Why a typed time is refused: outside the window the field offers, or between two steps. The
 * time stays in the field next to the reason, like a refused date does.
 */
export function timeError(
	time: { hour: number; minute: number },
	fromMinutes = DEFAULT_FROM_MINUTES,
	toMinutes = DEFAULT_TO_MINUTES,
	step = TIME_STEP_MINUTES,
): string | null {
	const minutes = time.hour * 60 + time.minute;

	if (minutes < fromMinutes || minutes > toMinutes) {
		return `The time must be between ${formatTimeOfDay(minutesToTime(fromMinutes))} and ${formatTimeOfDay(minutesToTime(toMinutes))}.`;
	}

	if (minutes % step !== 0) {
		return `Minutes go in steps of ${step}.`;
	}

	return null;
}
