import { ClipboardCheck } from "lucide-react";
import type { TimeSheetProjection } from "@/api/models";
import { EmptyState } from "@/components/ui";
import type { MonthInView } from "../../types";
import { useGetTimeSheet } from "../hooks";
import { type QueueEntry, SheetQueue } from "./SheetQueue";
import { TimeSheetPanel } from "./TimeSheetPanel";

interface Props {
	month: MonthInView;
	entries: QueueEntry[];
	selected: string | undefined;
	canSettle: boolean;
	emptyTitle: string;
	emptyDescription: string;
	onSelect: (userId: string) => void;
	onApprove: (sheet: TimeSheetProjection) => void;
	onReturn: (sheet: TimeSheetProjection) => void;
	onSettle: (sheet: TimeSheetProjection) => void;
	onComment: (sheet: TimeSheetProjection) => void;
}

/**
 * The shape both decision screens share: a list of months on the left, the one being read on the
 * right. The sheet itself is fetched per person rather than carried in the list, because neither
 * the monitoring rows nor the settlement list is the whole month - only the panel needs the days.
 */
export function TwoPaneSheets({
	month,
	entries,
	selected,
	canSettle,
	emptyTitle,
	emptyDescription,
	onSelect,
	onApprove,
	onReturn,
	onSettle,
	onComment,
}: Props) {
	/* Nothing picked yet means the first one - a queue that opens blank asks for an extra click. */
	const active = entries.some((entry) => entry.userId === selected)
		? selected
		: entries.at(0)?.userId;

	const sheet = useGetTimeSheet(active, month);

	if (entries.length === 0) {
		return (
			<EmptyState title={emptyTitle} description={emptyDescription}>
				<ClipboardCheck size={24} />
			</EmptyState>
		);
	}

	return (
		<div className="time-sheet-approvals">
			<SheetQueue entries={entries} selected={active} onSelect={onSelect} />

			{sheet.data ? (
				<TimeSheetPanel
					sheet={sheet.data}
					month={month}
					canSettle={canSettle}
					onApprove={onApprove}
					onReturn={onReturn}
					onSettle={onSettle}
					onComment={onComment}
				/>
			) : null}
		</div>
	);
}
