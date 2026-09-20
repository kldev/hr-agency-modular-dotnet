import { Pencil, UserMinus } from "lucide-react";
import { useRef, useState } from "react";
import { toast } from "sonner";
import type { TeamMemberSnapshot, TeamProjection } from "@/api/models";
import { Button, ConfirmDialog, DetailOverviewHeader, ItemMark } from "@/components/ui";
import { ActionMenu } from "@/components/ui/ActionMenu";
import { useRemoveTeamMember } from "../../pages/hooks";
import { teamRoles } from "../../types";
import AddMemberDrawer, { type AddMemberFormCommand } from "./AddMemberDrawer";
import ChangeMemberRoleDrawer, { type ChangeMemberRoleFormCommand } from "./ChangeMemberRoleDrawer";

interface TeamMembersSectionProps {
	team: TeamProjection;
	onRefresh: () => void;
}

export function TeamMembersSection({ team, onRefresh }: TeamMembersSectionProps) {
	const addRef = useRef<AddMemberFormCommand>(null);
	const roleRef = useRef<ChangeMemberRoleFormCommand>(null);

	const [pendingRemoval, setPendingRemoval] = useState<TeamMemberSnapshot | null>(null);

	const { mutation: removeMutation } = useRemoveTeamMember({
		onSuccess: () => {
			removeMutation.reset();
			toast.success("Member removed");
			setPendingRemoval(null);
			onRefresh();
		},
	});

	/*
	 * The domain refuses to empty a team, so the last member's Remove is disabled here rather than
	 * letting the click come back as a 400 the person could not have anticipated.
	 */
	const isLastMember = team.members.length <= 1;

	return (
		<>
			<div className="data-overview">
				<DetailOverviewHeader
					title="Members"
					description="Who is on this team, and what they do on it."
					onAdd={() => addRef.current?.add(team.id, team.name)}
				/>

				<ul className="data-content-list">
					{team.members.map((member) => (
						<li key={member.user.id} className="flex items-center gap-3 py-2">
							<ItemMark name={member.user.fullname ?? member.user.email} />

							<div className="min-w-0 flex-1">
								<div className="data-name">
									{member.user.fullname ?? `${member.user.firstName} ${member.user.lastName}`}
								</div>
								<div className="text-xs text-(--color-text-muted)">{member.user.email}</div>
							</div>

							<span className="data-detail-number">{teamRoles[member.role]}</span>

							<ActionMenu
								ariaLabel={`Actions for ${member.user.email}`}
								actions={[
									{
										label: "Change team role",
										icon: Pencil,
										action: () =>
											roleRef.current?.changeRole({
												teamId: team.id,
												userId: member.user.id,
												memberName:
													member.user.fullname ??
													`${member.user.firstName} ${member.user.lastName}`,
												currentRole: member.role,
											}),
									},
									{
										label: "Remove from team",
										icon: UserMinus,
										disabled: isLastMember,
										action: () => setPendingRemoval(member),
									},
								]}
							/>
						</li>
					))}
				</ul>

				{isLastMember ? (
					<p className="mt-2 text-xs text-(--color-text-muted)">
						A team always keeps at least one member. Add somebody else before removing the last one.
					</p>
				) : null}

				<div className="mt-4">
					<Button variant="secondary" onClick={() => addRef.current?.add(team.id, team.name)}>
						Add member
					</Button>
				</div>
			</div>

			<ConfirmDialog
				open={pendingRemoval !== null}
				title="Remove from team"
				description={`${
					pendingRemoval?.user.fullname ?? pendingRemoval?.user.email ?? "This person"
				} will be left without a team. They can be added to another one afterwards.`}
				confirmLabel="Remove"
				loading={removeMutation.isPending}
				onClose={() => setPendingRemoval(null)}
				onConfirm={() => {
					if (pendingRemoval) {
						removeMutation.mutate({ teamId: team.id, userId: pendingRemoval.user.id });
					}
				}}
			/>

			<AddMemberDrawer ref={addRef} onSuccess={onRefresh} />
			<ChangeMemberRoleDrawer ref={roleRef} onSuccess={onRefresh} />
		</>
	);
}
