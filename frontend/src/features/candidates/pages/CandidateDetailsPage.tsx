import { useQueryClient } from "@tanstack/react-query";
import { useRef } from "react";
import { MessagePreview } from "#/components/ui/MessagePreview";
import { type AddTagCommand, AddTagsDrawer } from "#/features/applications/pages/components";
import { applicationSources } from "#/features/applications/types";
import { CandidateTimeline } from "#/features/timeline";
import { candidatesKeys } from "@/api/query-keys";
import {
	AuditInformation,
	DataDetails,
	DetailsHeader,
	DetailsListSection,
	DetailsLoading,
	type TabDefinition,
	TabPanel,
	Tabs,
} from "@/components/ui";
import { DataDetailsLayout, DetailItem } from "@/components/ui/details/DataDetails";
import { EditCandidateDrawer, type EditCandidateFormCommand } from "../components";
import { useGetCandidate } from "../hooks";

export type CandidateTab = "profile" | "timeline";

export const candidateTabs: readonly CandidateTab[] = ["profile", "timeline"];

const tabs: TabDefinition<CandidateTab>[] = [
	{ id: "profile", label: "Profile" },
	{ id: "timeline", label: "Timeline" },
];

interface CandidateDetailsPageProps {
	id: string;
	tab: CandidateTab;
	onTabChange: (tab: CandidateTab) => void;
}

const CandidateDetailsPage: React.FC<CandidateDetailsPageProps> = ({ id, tab, onTabChange }) => {
	const formRef = useRef<EditCandidateFormCommand>(null);
	const tagRef = useRef<AddTagCommand>(null);
	const queryClient = useQueryClient();

	const query = useGetCandidate(id);

	if (!id || query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !query.data} />
		);
	}

	/* An edit or a tag is also a line in the timeline. */
	const refetch = () => {
		query.refetch();
		void queryClient.invalidateQueries({ queryKey: candidatesKeys.timeline(id) });
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

			<Tabs value={tab} tabs={tabs} onChange={onTabChange} label="Candidate sections" />

			<DataDetailsLayout
				main={
					<TabPanel id={tab}>
						{tab === "profile" ? (
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
						) : null}

						{tab === "timeline" ? <CandidateTimeline candidateId={candidate.id} /> : null}
					</TabPanel>
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
