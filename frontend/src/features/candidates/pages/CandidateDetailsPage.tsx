import { useRef } from "react";
import { MessagePreview } from "#/components/ui/MessagePreview";
import { type AddTagCommand, AddTagsDrawer } from "#/features/applications/pages/components";
import { applicationSources } from "#/features/applications/types";
import {
	AuditInformation,
	DataDetails,
	DetailsHeader,
	DetailsListSection,
	DetailsLoading,
} from "@/components/ui";
import { DataDetailsLayout, DetailItem } from "@/components/ui/details/DataDetails";
import { EditCandidateDrawer, type EditCandidateFormCommand } from "../components";
import { useGetCandidate } from "../hooks";

const CandidateDetailsPage: React.FC<{ id: string }> = ({ id }) => {
	const formRef = useRef<EditCandidateFormCommand>(null);
	const tagRef = useRef<AddTagCommand>(null);

	const query = useGetCandidate(id);

	if (!id || query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !query.data} />
		);
	}

	const refetch = () => {
		query.refetch();
	};

	const candidate = query.data;

	const tags = candidate?.tags?.length ? candidate.tags.map((z) => z.name) : [];

	return (
		<DataDetails>
			<DetailsHeader
				name={candidate.fullName}
				detailsAddons={
					<div className="data-details-header-meta">
						<span className="data-details-header-info">{applicationSources[candidate.source]}</span>
					</div>
				}
				onEdit={() => {
					formRef.current?.edit(candidate.id);
				}}
			/>

			<DataDetailsLayout
				main={
					<section className="data-details-section">
						<div className="data-details-section-header">
							<div>
								<h2>Applicant</h2>
								<p>Candidate contact information</p>
							</div>
						</div>

						<dl className="data-details-list">
							<DetailItem label="First name">{candidate.firstName || "-"}</DetailItem>

							<DetailItem label="Last name">{candidate.lastName || "-"}</DetailItem>

							<DetailItem label="Email">
								<a href={`mailto:${candidate.email}`}>{candidate.email}</a>
							</DetailItem>

							<DetailItem label="Phone">
								<a href={`tel:${candidate.phoneNumber}`}>{candidate.phoneNumber || "-"}</a>
							</DetailItem>

							<DetailItem label="Source">{applicationSources[candidate.source]}</DetailItem>
							<DetailItem label=""> </DetailItem>
						</dl>
						<DetailItem label="Note">
							<MessagePreview message={candidate.note} />
						</DetailItem>
					</section>
				}
				sidebar={
					<>
						<AuditInformation
							createdAt={candidate.createdAt}
							createdBy={candidate.createdBy}
							modifiedAt={candidate.modifiedAt}
							modifiedBy={candidate.modifiedBy}
						/>
						<div className="data-content-lists ">
							<DetailsListSection
								title="Tags"
								items={tags}
								className="short-items-section"
								onAdd={() => {
									tagRef.current?.addTag(
										candidate.id,
										candidate.fullName || candidate.email,
										"candidate",
									);
								}}
							/>
						</div>
					</>
				}
			/>

			<EditCandidateDrawer ref={formRef} onSuccess={refetch} />
			<AddTagsDrawer ref={tagRef} onSuccess={refetch} />
		</DataDetails>
	);
};

export default CandidateDetailsPage;
