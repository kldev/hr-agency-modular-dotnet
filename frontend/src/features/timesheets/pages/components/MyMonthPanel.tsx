import { Info, MessageSquare, Send } from "lucide-react";
import { useRef, useState } from "react";
import { toast } from "sonner";
import { useGetAgencyEmployment } from "#/features/agency-employment/pages/hooks";
import { contractRequiresTimeRecord } from "#/features/agency-employment/types";
import type { TimeSheetProjection, WorkDay } from "@/api/models";
import { Button, ConfirmDialog, TimeSheetStatusBadge } from "@/components/ui";
import {
	CommentOnTimeSheetDrawer,
	type CommentOnTimeSheetFormCommand,
	SaveWorkDayDrawer,
	type SaveWorkDayFormCommand,
} from "../../drawers";
import {
	formatMinutes,
	isTimeSheetEditable,
	type MonthInView,
	monthLabel,
	noEmploymentMessage,
	notCoveredMessage,
	submitWarning,
	workingDaysInMonth,
} from "../../types";
import { useSubmitTimeSheet } from "../hooks";
import { TimeSheetCalendar } from "./TimeSheetCalendar";
import { TimeSheetComments } from "./TimeSheetComments";
import { TimeSheetDayList } from "./TimeSheetDayList";

interface Props {
	month: MonthInView;
	userId: string;
	sheet: TimeSheetProjection | null;
	loading: boolean;
	onChanged: () => void;
}

export function MyMonthPanel({ month, userId, sheet, loading, onChanged }: Props) {
	const dayRef = useRef<SaveWorkDayFormCommand>(null);
	const commentRef = useRef<CommentOnTimeSheetFormCommand>(null);
	const [confirmSubmit, setConfirmSubmit] = useState(false);

	/*
	 * Asked so the page can explain itself rather than let the first click answer 400: somebody on
	 * B2B owes no hours, and somebody with no record at all has no month to record them against.
	 */
	const employment = useGetAgencyEmployment(userId);

	const { mutation: submit, waiting } = useSubmitTimeSheet({
		onSuccess: () => {
			setConfirmSubmit(false);
			onChanged();
		},

		onError: () => {
			setConfirmSubmit(false);
			toast.error("The month could not be sent");
		},
	});

	const days = sheet?.days ?? [];
	const totalMinutes = Number(sheet?.totalMinutes ?? 0);
	const status = sheet?.status ?? null;

	const covered = employment.data ? contractRequiresTimeRecord[employment.data.contractType] : null;
	const editable = isTimeSheetEditable(status) && covered !== false;

	/* Why it came back. The last word on a sheet in correction is the one that sent it back. */
	const lastComment = sheet?.comments?.at(-1) ?? null;

	if (!employment.isPending && !employment.data) {
		return (
			<div className="time-sheet-callout">
				<Info size={16} />
				<span>{noEmploymentMessage}</span>
			</div>
		);
	}

	return (
		<>
			{covered === false ? (
				<div className="time-sheet-callout">
					<Info size={16} />
					<span>{notCoveredMessage}</span>
				</div>
			) : null}

			{status === "Correction" && lastComment ? (
				<div className="time-sheet-callout">
					<Info size={16} />

					<span>
						<strong>Sent back for correction</strong> by {lastComment.author.firstName}{" "}
						{lastComment.author.lastName}: {lastComment.content}
					</span>
				</div>
			) : null}

			{/*
			 * Two shapes of the same month, one of them hidden by CSS. The calendar is worth keeping
			 * wherever it fits - a month is a grid in everybody's head - and the list is what a phone
			 * can actually show, so neither is a compromise for the other.
			 */}
			<TimeSheetCalendar
				month={month}
				days={days}
				readOnly={!editable}
				onSelectDay={(date, day: WorkDay | null) => dayRef.current?.saveDay({ date, day })}
			/>

			<TimeSheetDayList
				month={month}
				days={days}
				readOnly={!editable}
				onSelectDay={(date, day: WorkDay | null) => dayRef.current?.saveDay({ date, day })}
			/>

			<div className="time-sheet-summary">
				<dl className="time-sheet-summary-figures">
					<div className="time-sheet-summary-figure">
						<dt>Total</dt>
						<dd>{formatMinutes(totalMinutes)}</dd>
					</div>

					<div className="time-sheet-summary-figure">
						<dt>Days filled</dt>
						<dd>
							{days.length} / {workingDaysInMonth(month)}
						</dd>
					</div>

					<div className="time-sheet-summary-figure">
						<dt>Status</dt>
						<dd>{status ? <TimeSheetStatusBadge status={status} /> : "Not started"}</dd>
					</div>
				</dl>

				<div className="time-sheet-summary-actions">
					{/*
					 * The owner's side of the conversation. Worth having on every status, not only on a
					 * month that came back: answering "why is the 14th empty" before anybody has to ask is
					 * the cheapest version of this exchange.
					 */}
					{sheet ? (
						<Button
							variant="ghost"
							icon={<MessageSquare size={15} />}
							onClick={() => commentRef.current?.comment(sheet)}
						>
							Comment
						</Button>
					) : null}

					{/* Mirrors `EmptySheetMessage`: a month with nothing on it cannot be sent. */}
					<Button
						variant="primary"
						icon={<Send size={15} />}
						disabled={!editable || totalMinutes === 0 || loading}
						onClick={() => setConfirmSubmit(true)}
					>
						Submit for approval
					</Button>
				</div>
			</div>

			<ConfirmDialog
				open={confirmSubmit}
				danger={false}
				title={`Send ${monthLabel(month)} for approval?`}
				description={`${formatMinutes(totalMinutes)} over ${days.length} days. ${submitWarning}`}
				confirmLabel="Send"
				loading={submit.isPending || waiting}
				onConfirm={() => submit.mutate({ year: month.year, month: month.month })}
				onClose={() => setConfirmSubmit(false)}
			/>

			{/* Everything said about this month, by whoever said it - not only the last word. */}
			{sheet ? <TimeSheetComments comments={sheet.comments} /> : null}

			<SaveWorkDayDrawer ref={dayRef} onSuccess={onChanged} />
			<CommentOnTimeSheetDrawer ref={commentRef} onSuccess={onChanged} />
		</>
	);
}
