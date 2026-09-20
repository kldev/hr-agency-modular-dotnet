import { useParams } from "@tanstack/react-router";
import { useRef } from "react";
import { AuditInformation, DetailsHeader, DetailsLoading } from "@/components/ui";
import { DataDetails, DataDetailsLayout } from "@/components/ui/details/DataDetails";
import { RenameTeamDrawer, type RenameTeamFormCommand, TeamMembersSection } from "../components";
import { useGetTeam } from "./hooks";

export function TeamDetailsPage() {
	const { id } = useParams({ from: "/app/teams/$id" });
	const renameRef = useRef<RenameTeamFormCommand>(null);

	const query = useGetTeam(id);

	if (!id || query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !query.data} />
		);
	}

	const team = query.data;

	return (
		<>
			<DataDetails>
				<DetailsHeader
					name={team.name}
					onEdit={() => renameRef.current?.rename(team.id, team.name)}
				/>

				<DataDetailsLayout
					main={
						<section className="data-details-section">
							<TeamMembersSection
								team={team}
								onRefresh={() => {
									void query.refetch();
								}}
							/>
						</section>
					}
					sidebar={
						<AuditInformation
							createdAt={team.createdAt}
							createdBy={team.createdBy}
							modifiedAt={team.modifiedAt}
							modifiedBy={team.modifiedBy}
						/>
					}
				/>
			</DataDetails>

			<RenameTeamDrawer
				ref={renameRef}
				onSuccess={() => {
					void query.refetch();
				}}
			/>
		</>
	);
}
