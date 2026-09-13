export function parseScheduledAt(value: string): {
	date: Date | null;
	time: string;
} {
	if (!value) {
		return {
			date: null,
			time: "",
		};
	}

	const [datePart, timePart] = value.split("T");

	if (!datePart) {
		return {
			date: null,
			time: "",
		};
	}

	const [year, month, day] = datePart.split("-").map(Number);

	if (!year || !month || !day) {
		return {
			date: null,
			time: "",
		};
	}

	return {
		date: new Date(year, month - 1, day),
		time: timePart?.slice(0, 5) ?? "",
	};
}
