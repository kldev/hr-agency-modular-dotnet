import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui";
import type { CalendarRange } from "./type";

interface Props {
	title: string;
	range: CalendarRange;

	onRangeChange: (range: CalendarRange) => void;

	onPrevious: () => void;
	onNext: () => void;
	onToday: () => void;
}

export function InterviewsCalendarToolbar({
	title,
	range,
	onRangeChange,
	onPrevious,
	onNext,
	onToday,
}: Props) {
	return (
		<header className="interviews-calendar-toolbar">
			<div className="interviews-calendar-navigation">
				<Button variant="secondary" onClick={onToday}>
					Today
				</Button>

				<Button
					variant="ghost"
					className="button-icon"
					aria-label="Previous period"
					onClick={onPrevious}
				>
					<ChevronLeft size={16} />
				</Button>

				<Button variant="ghost" className="button-icon" aria-label="Next period" onClick={onNext}>
					<ChevronRight size={16} />
				</Button>

				<h2 className="interviews-calendar-title">{title}</h2>
			</div>

			<fieldset className="interviews-calendar-view-switch" aria-label="Calendar view">
				<button
					type="button"
					className={
						range === "week"
							? "interviews-calendar-view-button active"
							: "interviews-calendar-view-button"
					}
					onClick={() => onRangeChange("week")}
				>
					Week
				</button>

				<button
					type="button"
					className={
						range === "month"
							? "interviews-calendar-view-button active"
							: "interviews-calendar-view-button"
					}
					onClick={() => onRangeChange("month")}
				>
					Month
				</button>

				<button
					type="button"
					className={
						range === "quarter"
							? "interviews-calendar-view-button active"
							: "interviews-calendar-view-button"
					}
					onClick={() => onRangeChange("quarter")}
				>
					3 months
				</button>
			</fieldset>
		</header>
	);
}
