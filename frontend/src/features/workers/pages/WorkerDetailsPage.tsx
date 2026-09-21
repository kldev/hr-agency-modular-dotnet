import { useParams } from "@tanstack/react-router";
import type React from "react";
import type { UserSnapshot } from "@/api/models";
import { getCountryLabel } from "@/components/labels";
import {
	AuditInformation,
	DetailsHeader,
	DetailsLoading,
	WorkerStatusBadge,
} from "@/components/ui";
import { DataDetails, DataDetailsLayout } from "@/components/ui/details/DataDetails";
import {
	WorkerAssignmentsSection,
	WorkerAuthorisationsSection,
	WorkerContactSection,
	WorkerDocumentsSection,
	WorkerIdentitySection,
	WorkerPipelineSidebar,
} from "./components/details";
import { useGetWorker } from "./hooks";

const WorkerDetailsPage: React.FC = () => {
	const { id } = useParams({ from: "/app/workers/$id" });

	const query = useGetWorker(id);
	const worker = query.data;

	if (!id || query.isLoading || query.isError || !worker) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !worker} />
		);
	}

	return (
		<DataDetails>
			<DetailsHeader
				name={worker.fullName ?? ""}
				detailsAddons={
					<>
						<WorkerStatusBadge status={worker.status ?? "Recruitment"} />

						<span className="badge badge-inactive">
							{getCountryLabel(worker.citizenship ?? "")}
						</span>
					</>
				}
			/>

			<DataDetailsLayout
				main={
					<>
						<section className="data-details-section">
							<WorkerIdentitySection worker={worker} />
						</section>

						<section className="data-details-section">
							<WorkerContactSection worker={worker} />
						</section>

						<section className="data-details-section">
							<WorkerDocumentsSection worker={worker} />
						</section>

						{/* Only for somebody the legalisation rules apply to - see the section itself. */}
						{worker.requiresLegalisation ? (
							<section className="data-details-section">
								<WorkerAuthorisationsSection worker={worker} />
							</section>
						) : null}

						<section className="data-details-section">
							<WorkerAssignmentsSection worker={worker} />
						</section>
					</>
				}
				sidebar={
					<>
						<section className="data-details-section">
							<WorkerPipelineSidebar worker={worker} />
						</section>

						<AuditInformation
							createdAt={worker.createdAt ?? ""}
							createdBy={worker.createdBy as UserSnapshot}
							modifiedAt={worker.modifiedAt ?? null}
							modifiedBy={worker.modifiedBy ?? null}
						/>
					</>
				}
			/>
		</DataDetails>
	);
};

export default WorkerDetailsPage;
