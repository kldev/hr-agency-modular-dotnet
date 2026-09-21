import type { WorkerProjection } from "@/api/models";
import { getCountryLabel } from "@/components/labels";
import { DetailItem, DetailOverviewHeader } from "@/components/ui";
import { formatDate } from "@/utlis/dateUtils";
import { identityDocumentKinds } from "../../../types";

interface WorkerIdentitySectionProps {
	worker: WorkerProjection;
}

/** Everything that is true of the human being, as opposed to where they happen to work. */
export function WorkerIdentitySection({ worker }: WorkerIdentitySectionProps) {
	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Identity"
				description="Who this person is. None of it changes when they move to another project."
			/>

			<dl className="data-details-list">
				<DetailItem label="Full name">{worker.fullName}</DetailItem>

				<DetailItem label="Date of birth">
					{worker.dateOfBirth ? formatDate(worker.dateOfBirth) : "—"}
				</DetailItem>

				<DetailItem label="Citizenship">
					{getCountryLabel(worker.citizenship ?? "")}
					{worker.requiresLegalisation ? " · needs legalisation" : " · free movement"}
				</DetailItem>

				<DetailItem label="Document">
					{worker.identityDocumentKind ? identityDocumentKinds[worker.identityDocumentKind] : "—"}
				</DetailItem>

				<DetailItem label="Document number">{worker.identityDocumentNumber}</DetailItem>

				<DetailItem label="Issued by">
					{getCountryLabel(worker.identityDocumentIssuingCountry ?? "")}
				</DetailItem>

				<DetailItem label="Valid until">
					{worker.identityDocumentValidUntil
						? formatDate(worker.identityDocumentValidUntil)
						: "No expiry"}
				</DetailItem>
			</dl>
		</div>
	);
}
