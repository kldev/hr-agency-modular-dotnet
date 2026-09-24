import { History } from "lucide-react";
import { useState } from "react";
import type { FormVersionSummary } from "#/api/models";
import { Button, EmptyState } from "#/components/ui";
import { formatDateTime } from "#/utlis/dateUtils";
import { useGetFormVersion } from "../hooks";
import { DynamicReview } from "../renderer/DynamicReview";

/**
 * What went out, version by version. A version never changes after publication, so this is also
 * the answer to "what did the people who filled in v1 actually see".
 */
export function VersionsTab({
	formId,
	versions,
}: {
	formId: string;
	versions: readonly FormVersionSummary[];
}) {
	const [selected, setSelected] = useState<number | null>(
		versions[0] ? Number(versions[0].version) : null,
	);
	const version = useGetFormVersion(formId, selected);

	if (versions.length === 0) {
		return (
			<EmptyState title="Not published yet" description="Publishing the draft makes version 1.">
				<History size={24} />
			</EmptyState>
		);
	}

	return (
		<div className="grid gap-5 lg:grid-cols-[260px_minmax(0,1fr)]">
			<ul className="data-details-section flex flex-col gap-1 p-3">
				{versions.map((candidate) => (
					<li key={String(candidate.version)}>
						<Button
							variant={Number(candidate.version) === selected ? "primary" : "ghost"}
							className="w-full justify-start"
							onClick={() => setSelected(Number(candidate.version))}
						>
							v{String(candidate.version)} · {formatDateTime(candidate.publishedAt)}
						</Button>
					</li>
				))}
			</ul>

			<section className="data-details-section p-4">
				{version.data ? (
					<>
						<p className="form-hint mb-4">
							Published {formatDateTime(version.data.publishedAt)} by{" "}
							{version.data.publishedBy.fullname}.
						</p>
						<DynamicReview pages={version.data.pages} answers={[]} />
					</>
				) : null}
			</section>
		</div>
	);
}
