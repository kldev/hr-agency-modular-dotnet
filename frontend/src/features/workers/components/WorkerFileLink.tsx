import { Link } from "@tanstack/react-router";
import { HardHat } from "lucide-react";

interface Props {
	workerId: string | null | undefined;
}

/**
 * Whether a workers' file was opened from this candidate or application, and a way into it. Shared
 * by both recruitment lists, because the answer means the same on both: somebody already took this
 * person on, and their file is where the rest happens.
 */
export function WorkerFileLink({ workerId }: Props) {
	if (!workerId) return <span className="text-(--color-text-muted)">—</span>;

	return (
		<Link
			to="/app/workers/$id"
			params={{ id: workerId }}
			search={{ search: undefined, tab: undefined }}
			className="badge badge-active inline-flex items-center gap-1"
			title="Open the worker's file"
		>
			<HardHat size={12} />
			Worker
		</Link>
	);
}
