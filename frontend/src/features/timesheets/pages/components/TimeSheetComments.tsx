import type { TimeSheetComment } from "@/api/models";
import { formatDateTime } from "@/utlis/dateUtils";

interface Props {
	comments: TimeSheetComment[];
}

/**
 * The thread on a month, as everybody involved sees it. Shared by the panel a supervisor reads and
 * by the owner's own tab, because a conversation shown differently to the two people having it is
 * two conversations.
 */
export function TimeSheetComments({ comments }: Props) {
	if (comments.length === 0) return null;

	return (
		<div className="time-sheet-comments">
			{comments.map((comment) => (
				<div key={`${comment.at}-${comment.author.id}`} className="time-sheet-comment">
					<div className="time-sheet-comment-meta">
						<strong>
							{comment.author.firstName} {comment.author.lastName}
						</strong>

						{/* The role is frozen on the comment: who they were when they wrote it. */}
						<span>{comment.authorRole}</span>

						<span>{formatDateTime(comment.at)}</span>
					</div>

					<div>{comment.content}</div>
				</div>
			))}
		</div>
	);
}
