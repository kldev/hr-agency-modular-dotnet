import type { TeamTimeSheetRow } from "@/api/models";
import { DetailItem, ItemMark, TimeSheetStatusOrNotStartedBadge } from "@/components/ui";
import { formatDate } from "@/utlis/dateUtils";
import { formatMinutes, type MonthInView, workingDaysInMonth } from "../../types";

interface Props {
	rows: TeamTimeSheetRow[];
	month: MonthInView;
}

/**
 * The monitoring table as cards, for screens the six columns do not fit on. Same facts in the same
 * order, including the people who have written nothing - a row that is empty is the reason this
 * screen exists, so it cannot be the one that gets dropped to save space.
 */
export function TeamMonitoringCardList({ rows, month }: Props) {
	const workingDays = workingDaysInMonth(month);

	return (
		<div className="data-mobile-view">
			{rows.map((row) => {
				const name = `${row.user.firstName} ${row.user.lastName}`;

				return (
					<div
						key={row.userId}
						className="flex flex-col gap-3 pb-5 border mb-5 mt-5 shadow-subtle card-border"
					>
						<div className="data-detail-item flex flex-row items-center gap-3">
							<ItemMark name={name} />

							<div className="grow min-w-0">
								<dt>{name}</dt>
								<dd className="truncate">{row.user.email}</dd>
							</div>

							<TimeSheetStatusOrNotStartedBadge status={row.status} />
						</div>

						<dl className="data-details-list">
							<DetailItem label="Hours">{formatMinutes(Number(row.totalMinutes))}</DetailItem>

							<DetailItem label="Days">{`${Number(row.filledDays)} / ${workingDays}`}</DetailItem>

							<DetailItem label="Last entry">{formatDate(row.lastEntryOn)}</DetailItem>

							<DetailItem label="Sent">{formatDate(row.submittedAt)}</DetailItem>
						</dl>
					</div>
				);
			})}
		</div>
	);
}
