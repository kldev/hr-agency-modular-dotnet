import clsx from "clsx";
import type { TimeSheetStatus } from "@/api/models";
import { TimeSheetStatusBadge } from "@/components/ui";

export type QueueEntry = {
	userId: string;
	name: string;
	meta: string;
	status: TimeSheetStatus;
};

interface Props {
	entries: QueueEntry[];
	selected: string | undefined;
	onSelect: (userId: string) => void;
}

/** The list half of the two-pane screens: pick a person, read their month next to it. */
export function SheetQueue({ entries, selected, onSelect }: Props) {
	return (
		<div className="time-sheet-queue">
			{entries.map((entry) => (
				<button
					type="button"
					key={entry.userId}
					className={clsx("time-sheet-queue-item", { "is-selected": entry.userId === selected })}
					onClick={() => onSelect(entry.userId)}
				>
					<span className="time-sheet-queue-name">{entry.name}</span>

					<span className="time-sheet-queue-meta">{entry.meta}</span>

					<span>
						<TimeSheetStatusBadge status={entry.status} />
					</span>
				</button>
			))}
		</div>
	);
}
