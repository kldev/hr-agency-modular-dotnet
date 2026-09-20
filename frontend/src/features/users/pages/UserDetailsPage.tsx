import { Link, useParams } from "@tanstack/react-router";
import { useRef } from "react";
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
import { useGetUser } from "./hooks";

export function UserDetailsPage() {
	const { id } = useParams({ from: "/app/users/$id" });

	const editRef = useRef<EditUserFormCommand>(null);
	const roleRef = useRef<ChangeUserRoleFormCommand>(null);
	const teamRef = useRef<ChangeUserTeamFormCommand>(null);

	const query = useGetUser(id);

	if (!id || query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !query.data} />
		);
	}

	const user = query.data;
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

									<DetailItem label="Organization role">{user.role}</DetailItem>

									<DetailItem label="Organization">{user.organization.name}</DetailItem>

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
