import type { InfiniteData, UseInfiniteQueryResult } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import type { TimelineItem, TimelineSlice } from "@/api/models";
import { LoadMore } from "@/components/ui";
import { formatDateTime } from "@/utlis";
import { describeTimelineEntry, timelineEntryIcons, timelineEntryLabels } from "./types";
import { useApplicationTimeline, useCandidateTimeline } from "./useTimeline";
import "./timeline.css";

type TimelineQuery = UseInfiniteQueryResult<InfiniteData<TimelineSlice>>;

interface RecruitmentTimelineProps {
	query: TimelineQuery;
	description: string;
	/** A candidate's timeline spans several applications, so each entry says which one. */
	showApplication: boolean;
}

function TimelineEntry({
	item,
	showApplication,
}: {
	item: TimelineItem;
	showApplication: boolean;
}) {
	const Icon = timelineEntryIcons[item.type];
	const detail = describeTimelineEntry(item);
	const applicationId = showApplication ? item.jobApplicationId : null;

	return (
		<li className="timeline-entry">
			<span className="timeline-entry-marker">
				<Icon size={14} />
			</span>

			<div className="timeline-entry-content">
				<div className="timeline-entry-heading">
					<span className="timeline-entry-title">{timelineEntryLabels[item.type]}</span>
					<time dateTime={item.occurredAt}>{formatDateTime(item.occurredAt)}</time>
				</div>

				{detail ? <div className="timeline-entry-detail">{detail}</div> : null}

				{applicationId || item.authorName ? (
					<div className="timeline-entry-meta">
						{applicationId ? (
							<>
								<Link
									to="/app/applications/$id"
									params={{ id: applicationId }}
									search={{
										status: undefined,
										source: undefined,
										search: undefined,
										tab: undefined,
									}}
								>
									{item.jobPostTitle || "Application"}
								</Link>
								{item.jobPostId ? (
									<Link
										to="/app/jobs/$id"
										params={{ id: item.jobPostId }}
										search={{ search: undefined, status: undefined }}
										className="timeline-entry-secondary-link"
									>
										Job post
									</Link>
								) : null}
							</>
						) : null}

						{item.authorName ? <span>{item.authorName}</span> : null}
					</div>
				) : null}
			</div>
		</li>
	);
}

/**
 * The history of a candidate or of one application, newest first. Compact on purpose - a line per
 * fact, not a card - and read-only: the entries are what happened, nothing here changes them.
 */
export function RecruitmentTimeline({
	query,
	description,
	showApplication,
}: RecruitmentTimelineProps) {
	const items = query.data?.pages.flatMap((page) => page.content) ?? [];

	return (
		<section className="data-details-section mt-1">
			<div className="data-details-section-header">
				<div>
					<h2>Timeline</h2>
					<p>{description}</p>
				</div>
			</div>

			{query.isLoading ? <div className="data-details-loading">Loading ...</div> : null}

			{query.isError ? (
				<div className="data-details-error">Unable to load the timeline.</div>
			) : null}

			{!query.isLoading && !query.isError && items.length === 0 ? (
				<div className="data-details-empty">Nothing has happened yet.</div>
			) : null}

			{items.length ? (
				<ol className="timeline-list">
					{items.map((item) => (
						<TimelineEntry key={item.id} item={item} showApplication={showApplication} />
					))}
				</ol>
			) : null}

			<LoadMore
				hasNext={Boolean(query.hasNextPage)}
				loading={query.isFetchingNextPage}
				onClick={() => query.fetchNextPage()}
			/>
		</section>
	);
}

export function CandidateTimeline({ candidateId }: { candidateId: string }) {
	return (
		<RecruitmentTimeline
			query={useCandidateTimeline(candidateId)}
			description="Everything that happened to this candidate, across all applications"
			showApplication
		/>
	);
}

export function ApplicationTimeline({ jobApplicationId }: { jobApplicationId: string }) {
	return (
		<RecruitmentTimeline
			query={useApplicationTimeline(jobApplicationId)}
			description="The history of this application and its interviews"
			showApplication={false}
		/>
	);
}
