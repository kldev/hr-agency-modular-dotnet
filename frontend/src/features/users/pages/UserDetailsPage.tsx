import { Link, useParams } from "@tanstack/react-router";
import { useRef } from "react";
import {
	useGetOrgStructure,
	useGetSubordinates,
	useGetSupervisor,
} from "#/features/org-structure/pages/hooks";
import { unitOfUser } from "#/features/org-structure/types";
import { AuditInformation, DetailsHeader, DetailsLoading } from "@/components/ui";
import {
	DataDetails,
	DataDetailsLayout,
	DetailItem,
	DetailOverviewHeader,
	EmailItem,
	PhoneItem,
} from "@/components/ui/details/DataDetails";
import { teamRoles } from "@/features/teams/types";
import {
	ChangeUserRoleDrawer,
	type ChangeUserRoleFormCommand,
	ChangeUserTeamDrawer,
	type ChangeUserTeamFormCommand,
	EditUserDrawer,
	type EditUserFormCommand,
} from "../components/form";
import { UserActions } from "../components/table/UserActions";
import { organizationRoleLabel } from "../types";
import { useGetUser } from "./hooks";

export function UserDetailsPage() {
	const { id } = useParams({ from: "/app/users/$id" });

	const editRef = useRef<EditUserFormCommand>(null);
	const roleRef = useRef<ChangeUserRoleFormCommand>(null);
	const teamRef = useRef<ChangeUserTeamFormCommand>(null);

	const query = useGetUser(id);

	/*
	 * Neither of these is on the user record, and that is the point: a unit is a fact about the chart
	 * and a supervisor is computed from it, so both are asked of the org structure rather than stored
	 * on the person. Nothing here is editable - moving somebody is done on the chart.
	 */
	const structure = useGetOrgStructure();
	const supervisor = useGetSupervisor(id);

	const unit = unitOfUser(structure.data?.units ?? [], id);
	const headsAUnit = (structure.data?.units ?? []).some((candidate) => candidate.headUserId === id);

	/* Only asked for somebody who heads something - for everybody else the answer is always none. */
	const subordinates = useGetSubordinates(id, true, headsAUnit);

	if (!id || query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !query.data} />
		);
	}

	const user = query.data;

	/*
	 * "Nobody above them" is an answer, not an empty state, so it must not appear while the question
	 * is still out. The chart is usually warm from the users list by the time somebody opens a person,
	 * while this lookup is keyed per user and never is - without the guard the row would state the
	 * wrong answer for a moment to everybody who does have a supervisor.
	 */
	const supervisorLabel = supervisor.isPending
		? null
		: supervisor.data
			? `${supervisor.data.fullName} · ${supervisor.data.unitName}`
			: unit
				? "Nobody above them"
				: null;

	/* Same reasoning: a count of nobody is a claim, and it would be wrong until the answer lands. */
	const subordinateCount = subordinates.isPending ? null : (subordinates.data?.length ?? 0);
	const refresh = () => {
		void query.refetch();
	};

	return (
		<>
			<DataDetails>
				<DetailsHeader
					name={user.fullName ?? `${user.firstName} ${user.lastName}`}
					onEdit={() => editRef.current?.edit(user)}
					detailsAddons={
						<UserActions
							mode="details"
							id={user.id}
							email={user.email}
							onEdit={() => editRef.current?.edit(user)}
							onChangeRole={() => roleRef.current?.changeRole(user)}
							onChangeTeam={() => teamRef.current?.changeTeam(user)}
						/>
					}
				/>

				<DataDetailsLayout
					main={
						<section className="data-details-section">
							<div className="data-overview">
								<DetailOverviewHeader
									title="User details"
									description="Contact data and what this person may do."
								/>

								<dl className="data-details-list">
									<EmailItem email={user.email} />

									<PhoneItem phone={user.phone} />

									<DetailItem label="Job title">{user.jobTitle}</DetailItem>

									<DetailItem label="Organization role">
										{organizationRoleLabel(user.role)}
									</DetailItem>

									<DetailItem label="Organization">{user.organization.name}</DetailItem>

									<DetailItem label="Unit">
										{unit ? (
											<Link
												to="/app/org-structure"
												search={{ unit: unit.unitId, includeArchived: undefined }}
											>
												{unit.name}
												{unit.headUserId === user.id ? " (heads it)" : ""}
											</Link>
										) : null}
									</DetailItem>

									{/* Computed from the chart on every read, which is why there is nothing to edit. */}
									<DetailItem label="Supervisor">{supervisorLabel}</DetailItem>

									{headsAUnit ? (
										<DetailItem label="Responsible for">
											{subordinateCount === null
												? null
												: `${subordinateCount} ${subordinateCount === 1 ? "person" : "people"}`}
										</DetailItem>
									) : null}

									<DetailItem label="Team">
										{user.team ? (
											<Link
												to="/app/teams/$id"
												params={{ id: user.team.id }}
												search={{ search: "" }}
											>
												{user.team.name} ({teamRoles[user.team.role]})
											</Link>
										) : null}
									</DetailItem>
								</dl>
							</div>
						</section>
					}
					sidebar={
						<AuditInformation
							createdAt={user.createdAt}
							createdBy={user.createdBy}
							modifiedAt={user.modifiedAt}
							modifiedBy={user.modifiedBy}
						/>
					}
				/>
			</DataDetails>

			<EditUserDrawer ref={editRef} onSuccess={refresh} />
			<ChangeUserRoleDrawer ref={roleRef} onSuccess={refresh} />
			<ChangeUserTeamDrawer ref={teamRef} onSuccess={refresh} />
		</>
	);
}
