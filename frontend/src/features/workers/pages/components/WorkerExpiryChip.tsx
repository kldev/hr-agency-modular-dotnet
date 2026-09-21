import clsx from "clsx";
import { differenceInCalendarDays } from "date-fns";
import { AlertTriangle } from "lucide-react";
import { formatDate } from "@/utlis/dateUtils";

/** How close an expiry has to be before the register starts pointing at it. */
const EXPIRY_WARNING_DAYS = 30;

type WorkerExpiryChipProps = {
	identityDocumentValidUntil: string | null | undefined;
	nextAuthorisationExpiryOn: string | null | undefined;
};

type Soonest = { date: string; what: string } | null;

function soonest(
	identityDocumentValidUntil: string | null | undefined,
	nextAuthorisationExpiryOn: string | null | undefined,
): Soonest {
	const candidates: { date: string; what: string }[] = [];

	if (identityDocumentValidUntil) {
		candidates.push({ date: identityDocumentValidUntil, what: "Identity document" });
	}

	if (nextAuthorisationExpiryOn) {
		candidates.push({ date: nextAuthorisationExpiryOn, what: "Permission to work" });
	}

	if (candidates.length === 0) {
		return null;
	}

	return candidates.reduce((earliest, candidate) =>
		candidate.date < earliest.date ? candidate : earliest,
	);
}

/**
 * Why this register is worth opening: an expired identity document blocks the move to Employed, and
 * a lapsed permit means somebody is working who may not. Both have to be visible *before* the day
 * they matter, which is why the two dates are folded into one cell rather than two columns nobody
 * scans.
 */
export function WorkerExpiryChip({
	identityDocumentValidUntil,
	nextAuthorisationExpiryOn,
}: WorkerExpiryChipProps) {
	const next = soonest(identityDocumentValidUntil, nextAuthorisationExpiryOn);

	if (!next) {
		return <span className="data-meta">—</span>;
	}

	const days = differenceInCalendarDays(new Date(next.date), new Date());
	const lapsed = days < 0;
	const soon = days <= EXPIRY_WARNING_DAYS;

	return (
		<span
			className={clsx("badge", lapsed ? "badge-cancelled" : soon ? "badge-new" : "badge-active")}
			title={`${next.what} ${lapsed ? "expired" : "valid until"} ${formatDate(next.date)}`}
		>
			{(lapsed || soon) && <AlertTriangle size={12} className="mr-1" />}
			{formatDate(next.date)}
		</span>
	);
}
