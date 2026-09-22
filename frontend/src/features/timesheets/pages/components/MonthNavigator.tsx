import { ChevronLeft, ChevronRight, RefreshCcw } from "lucide-react";
import { Button } from "@/components/ui";
import { currentMonth, isFutureMonth, type MonthInView, monthLabel, shiftMonth } from "../../types";

interface Props {
	month: MonthInView;
	onChange: (month: MonthInView) => void;
	loading: boolean;
	onRefresh: (page: number) => void;
}

/**
 * The month belongs to the page, not to a tab: switching from "my hours" to the team must not move
 * everybody back to today. Forward stops at the current month, because a month that has not
 * started cannot be filled in - mirrors `TimeSheetPeriod.HasStartedBy`.
 */
export function MonthNavigator({ month, onChange, onRefresh, loading }: Props) {
	const now = currentMonth();
	const isNow = month.year === now.year && month.month === now.month;

	return (
		<div className="month-navigator">
			<Button
				className="shrink-0"
				variant="secondary"
				icon={<RefreshCcw size={15} />}
				onClick={() => onRefresh(0)}
				loading={loading}
			>
				Refresh
			</Button>
			<Button
				variant="ghost"
				aria-label="Previous month"
				icon={<ChevronLeft size={16} />}
				onClick={() => onChange(shiftMonth(month, -1))}
			/>

			<span className="month-navigator-label">{monthLabel(month)}</span>

			<Button
				variant="ghost"
				aria-label="Next month"
				icon={<ChevronRight size={16} />}
				disabled={isFutureMonth(shiftMonth(month, 1))}
				onClick={() => onChange(shiftMonth(month, 1))}
			/>

			{isNow ? null : (
				<Button variant="secondary" onClick={() => onChange(now)}>
					Today
				</Button>
			)}
		</div>
	);
}
