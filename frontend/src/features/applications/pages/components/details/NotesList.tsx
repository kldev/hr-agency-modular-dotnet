import { useQuery } from "@tanstack/react-query";
import { getJobApplicationNotes } from "@/api/endpoints";
import { MessagePreview } from "@/components/ui/MessagePreview";
import { formatDateTimeIntl } from "@/utlis";
import "./notes.css";
import { PlusIcon } from "lucide-react";
import { jobDescriptionKeys } from "#/api";
import { Button } from "@/components/ui";

export function NotesList({ id, add }: { id: string; add: () => void }) {
	const notesQuery = useQuery({
		queryKey: jobDescriptionKeys.notes(id),
		queryFn: ({ signal }) => {
			if (!id) {
				throw new Error("Job application id is required");
			}

			return getJobApplicationNotes(id, undefined, signal);
		},
		enabled: Boolean(id),
	});

	if (!id) {
		return (
			<section className="data-details-section">
				<div className="data-details-section-header">
					<h2>Notes</h2>
				</div>

				<div className="data-details-empty">Notes not found.</div>
			</section>
		);
	}

	if (notesQuery.isLoading) {
		return (
			<section className="data-details-section">
				<div className="data-details-loading">Loading notes...</div>
			</section>
		);
	}

	if (notesQuery.isError || !notesQuery.data) {
		return (
			<section className="data-details-section">
				<div className="data-details-error">Unable to load notes.</div>
			</section>
		);
	}

	const notes = notesQuery.data;

	return (
		<section className="data-details-section notes-section">
			<div className="data-details-section-header">
				<h2>Notes</h2>

				<div className="flex flex-row gap-2 items-center justify-items-end">
					<Button variant="ghost" onClick={add} icon={<PlusIcon size={16} />}></Button>
					{notes.length > 0 && <span className="notes-count">{notes.length}</span>}
				</div>
			</div>

			{notes.length === 0 ? (
				<div className="data-details-empty">No notes have been added yet.</div>
			) : (
				<div className="notes-list">
					{notes.map((note) => (
						<article className="note-item" key={note.id}>
							<div className="note-content">
								<MessagePreview message={note.note} maxLength={300} />
							</div>

							<footer className="note-meta">
								<span className="note-author">{note.author}</span>

								<span className="note-separator" aria-hidden="true">
									·
								</span>

								<time dateTime={note.createdAt}>{formatDateTimeIntl(note.createdAt)}</time>
							</footer>
						</article>
					))}
				</div>
			)}
		</section>
	);
}
