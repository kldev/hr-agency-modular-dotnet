import { format } from "date-fns";
import { pl } from "date-fns/locale";

export function formatDateTime(value: string | null | undefined): string {
	if (!value) {
		return "—";
	}

	return format(new Date(value), "dd.MM.yyyy HH:mm", {
		locale: pl,
	});
}

export function formatDate(value: string | null | undefined): string {
	if (!value) {
		return "—";
	}

	return format(new Date(value), "dd.MM.yyyy", {
		locale: pl,
	});
}
