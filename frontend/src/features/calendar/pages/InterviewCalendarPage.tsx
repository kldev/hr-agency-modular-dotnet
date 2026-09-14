import { addMonths, endOfMonth, endOfWeek, startOfMonth, startOfWeek } from "date-fns";
import { CalendarDays, CheckCircle2, Plus, UserX } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { getInterviewsForDateRange } from "@/api/endpoints";
import type { InterviewProjection, InterviewStatus } from "@/api/models";
import { Button } from "@/components/ui/Button";

import { type CalendarRange, InterviewsCalendar } from "./components";
import "./interviews.css";
import { useNavigate } from "@tanstack/react-router";

export default function InterviewCalendarPage() {
	const [range, setRange] = useState<CalendarRange>("week");

	const [date, setDate] = useState(new Date());

	const [interviews, setInterviews] = useState<InterviewProjection[]>([]);

	const [loading, setLoading] = useState(false);
	const naviagation = useNavigate();

	useEffect(() => {
		let active = true;

		async function load() {
			setLoading(true);

			const { from, to } = getCalendarRange(date, range);

			try {
				const response = await getInterviewsForDateRange({
					fromDate: from.toISOString().split("T")[0],
					toDate: to.toISOString().split("T")[0],
				});

				if (active) {
					setInterviews(response);
				}
			} finally {
				if (active) {
					setLoading(false);
				}
			}
		}

		void load();

		return () => {
			active = false;
		};
	}, [date, range]);

	const stats = useMemo(() => {
		const now = new Date();

		const upcomingStatuses: InterviewStatus[] = [
			"Planned",
			"InProgress",
			"Rescheduled",
			"Confirmed",
		];
		const upcoming = interviews.filter(
			(interview) =>
				upcomingStatuses.indexOf(interview.status) > -1 && new Date(interview.scheduleAt) >= now,
		).length;

		const completed = interviews.filter((interview) => interview.status === "Completed").length;

		const noShows = interviews.filter((interview) => interview.status === "NoShow").length;

		return {
			upcoming,
			completed,
			noShows,
		};
	}, [interviews]);

	return (
		<main className="min-w-0 flex-1 overflow-auto">
			<div className="page">
				<div className="page-header">
					<div>
						<div className="breadcrumbs">
							<span>Recruitment</span>

							<span className="breadcrumb-separator">/</span>

							<span className="breadcrumb-current">Interviews</span>
						</div>

						<h1 className="page-title">Interviews</h1>

						<p className="page-description">Schedule and manage interviews with job applicants.</p>
					</div>

					<Button
						variant="primary"
						onClick={() => {
							naviagation({ to: "/app/applications" });
						}}
					>
						<Plus size={16} />
						Schedule interview
					</Button>
				</div>

				<div className="interview-stats">
					<InterviewStat
						icon={CalendarDays}
						label="Upcoming"
						value={stats.upcoming}
						description="In selected period"
					/>

					<InterviewStat
						icon={CheckCircle2}
						label="Completed"
						value={stats.completed}
						description="Completed interviews"
					/>

					<InterviewStat
						icon={UserX}
						label="No shows"
						value={stats.noShows}
						description="Candidates did not attend"
					/>
				</div>

				<section className="interviews-calendar-panel">
					{loading && (
						<div className="interviews-calendar-loading">
							<span className="spinner" />
							Loading interviews...
						</div>
					)}

					<InterviewsCalendar
						range={range}
						date={date}
						interviews={interviews}
						onRangeChange={setRange}
						onDateChange={setDate}
						onSelectInterview={(interview) => {
							console.log("Selected interview", interview);
						}}
						onSelectDate={(selectedDate) => {
							console.log("Selected date", selectedDate);
						}}
					/>
				</section>
			</div>
		</main>
	);
}

function InterviewStat({
	icon: Icon,
	label,
	value,
	description,
}: {
	icon: typeof CalendarDays;
	label: string;
	value: string | number;
	description: string;
}) {
	return (
		<div className="interview-stat">
			<div className="interview-stat-icon">
				<Icon size={17} />
			</div>

			<div>
				<div className="interview-stat-label">{label}</div>

				<div className="interview-stat-value">{value}</div>

				<div className="interview-stat-description">{description}</div>
			</div>
		</div>
	);
}

function getCalendarRange(date: Date, range: CalendarRange) {
	switch (range) {
		case "week":
			return {
				from: startOfWeek(date, {
					weekStartsOn: 1,
				}),
				to: endOfWeek(date, {
					weekStartsOn: 1,
				}),
			};

		case "month":
			return {
				from: startOfMonth(date),
				to: endOfMonth(date),
			};

		case "quarter":
			return {
				from: startOfMonth(date),
				to: endOfMonth(addMonths(date, 2)),
			};
	}
}
