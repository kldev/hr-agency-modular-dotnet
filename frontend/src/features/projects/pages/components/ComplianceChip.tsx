import clsx from "clsx";
import { differenceInCalendarDays } from "date-fns";
import { AlertTriangle } from "lucide-react";
import { formatDate } from "@/utlis/dateUtils";

/** How close an expiry has to be before the list starts pointing at it. */
const EXPIRY_WARNING_DAYS = 30;

type ComplianceChipProps = {
	outstanding: number | string;
	nextExpiryOn: string | null;
};

export function isExpiringSoon(nextExpiryOn: string | null) {
	if (!nextExpiryOn) {
		return false;
	}

	const days = differenceInCalendarDays(new Date(nextExpiryOn), new Date());

	return days <= EXPIRY_WARNING_DAYS;
}

/**
 * The whole point of the module in one cell: knowing that something lapses *before* it lapses.
 * An outstanding count of zero is worth showing too - it is the answer to "is this one fine?".
 */
export function ComplianceChip({ outstanding, nextExpiryOn }: ComplianceChipProps) {
	const count = Number(outstanding);
	const expiring = isExpiringSoon(nextExpiryOn);

	const title = expiring
		? `Next item expires on ${formatDate(nextExpiryOn)}`
		: count > 0
			? `${count} compliance ${count === 1 ? "item" : "items"} outstanding`
			: "Nothing outstanding";

	return (
		<span
			className={clsx(
				"badge",
				expiring ? "badge-cancelled" : count > 0 ? "badge-new" : "badge-active",
			)}
			title={title}
		>
			{expiring && <AlertTriangle size={12} className="mr-1" />}
			{count > 0 ? `${count} open` : "Clear"}
		</span>
	);
}
