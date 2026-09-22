import { Check, MessageSquare, Undo2, Wallet } from "lucide-react";
import type { TimeSheetProjection } from "@/api/models";
import { Button, TimeSheetStatusBadge } from "@/components/ui";
import { formatDate } from "@/utlis/dateUtils";
import {
	allowedTimeSheetTransitions,
	formatMinutes,
	type MonthInView,
	monthLabel,
	toTimeOfDay,
} from "../../types";
import { TimeSheetComments } from "./TimeSheetComments";

interface Props {
	sheet: TimeSheetProjection;
	month: MonthInView;
	/** Payroll sees the settle button; everybody else decides by the chart, which the backend checks. */
	canSettle: boolean;
	onApprove: (sheet: TimeSheetProjection) => void;
	onReturn: (sheet: TimeSheetProjection) => void;
	onSettle: (sheet: TimeSheetProjection) => void;
	onComment: (sheet: TimeSheetProjection) => void;
}

/**
 * One month, read. Which buttons appear comes from the status graph
 * (`allowedTimeSheetTransitions`), so a settled sheet offers nothing rather than offering a button
 * the backend would refuse.
 */
export function TimeSheetPanel({
	sheet,
	month,
	canSettle,
	onApprove,
	onReturn,
	onSettle,
	onComment,
}: Props) {
	const next = allowedTimeSheetTransitions[sheet.status];

	const person = `${sheet.user.firstName} ${sheet.user.lastName}`;

	return (
		<div className="time-sheet-panel">
			<div className="time-sheet-panel-header">
				<div>
					<h2>{person}</h2>

					<p className="page-description">
						{monthLabel(month)} · {formatMinutes(Number(sheet.totalMinutes ?? 0))} over{" "}
						{Number(sheet.filledDays ?? 0)} days · <TimeSheetStatusBadge status={sheet.status} />
					</p>
				</div>

				<div className="time-sheet-panel-actions">
					{next.includes("Approved") ? (
						<Button variant="primary" icon={<Check size={15} />} onClick={() => onApprove(sheet)}>
							Approve
						</Button>
					) : null}

					{next.includes("Correction") ? (
						<Button variant="secondary" icon={<Undo2 size={15} />} onClick={() => onReturn(sheet)}>
							Send back
						</Button>
					) : null}

					{canSettle && next.includes("Settled") ? (
						<Button variant="primary" icon={<Wallet size={15} />} onClick={() => onSettle(sheet)}>
							Settle
						</Button>
					) : null}

					<Button
						variant="ghost"
						icon={<MessageSquare size={15} />}
						onClick={() => onComment(sheet)}
					>
						Comment
					</Button>
				</div>
			</div>

			{/* In a container so the four columns scroll on a narrow screen instead of spilling out. */}
			<div className="table-container">
				<table className="table">
					<thead>
						<tr>
							<th className="table-header-md">Day</th>
							<th className="table-header-ssm">From</th>
							<th className="table-header-ssm">Worked</th>
							<th>Note</th>
						</tr>
					</thead>

					<tbody>
						{sheet.days.map((day) => (
							<tr key={day.date}>
								<td className="table-figure">{formatDate(day.date)}</td>
								<td className="table-figure">{toTimeOfDay(day.startsAt)}</td>
								<td className="table-figure">{formatMinutes(Number(day.minutes))}</td>
								<td>{day.note}</td>
							</tr>
						))}
					</tbody>
				</table>
			</div>

			<TimeSheetComments comments={sheet.comments} />
		</div>
	);
}
