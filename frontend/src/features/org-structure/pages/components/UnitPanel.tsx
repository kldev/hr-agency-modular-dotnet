import {
	Archive,
	ArrowRightLeft,
	LogIn,
	Pencil,
	Plus,
	UserMinus,
	UserRoundCog,
	X,
} from "lucide-react";
import type { OrgUnitMember, OrgUnitRow, UserProjection } from "@/api/models";
import { Avatar, Button } from "@/components/ui";
import { ActionMenu, type ActionMenuItem } from "@/components/ui/ActionMenu";
import { userAvatarUrl } from "@/features/users/avatar";
import { isAdmin } from "@/features/users/types";
import { useAuthStore } from "@/stores/authStore";
import {
	alreadyArchivedMessage,
	archiveBlockedReason,
	memberHeadsUnitMessage,
	nameOf,
	orgUnitKinds,
	rootCannotMoveMessage,
} from "../../types";

interface UnitPanelProps {
	unit: OrgUnitRow;
	units: OrgUnitRow[];
	resolveUser: (userId: string) => UserProjection | undefined;
	onRename: () => void;
	onMove: () => void;
	onArchive: () => void;
	onAssignHead: () => void;
	onClearHead: () => void;
	onAddMember: () => void;
	onRemoveMember: (member: OrgUnitMember) => void;
	onCreateChild: () => void;
	onImpersonate: (user: { id: string; fullName?: string | null }) => void;
	/** The file id of somebody's picture, or null - `useUserAvatars` explains why it is separate. */
	avatarOf: (userId: string) => string | null;
}

export function UnitPanel({
	unit,
	units,
	resolveUser,
	onRename,
	onMove,
	onArchive,
	onAssignHead,
	onClearHead,
	onAddMember,
	onRemoveMember,
	onCreateChild,
	onImpersonate,
	avatarOf,
}: UnitPanelProps) {
	const isRoot = unit.parentId === null;
	const head = unit.headUserId ? resolveUser(unit.headUserId) : undefined;

	const currentUser = useAuthStore((state) => state.user);
	const administrator = isAdmin(currentUser?.role);

	/*
	 * Signing in as somebody is offered here and not only in the user list because this is the screen
	 * that says who answers for whom - and approving a month follows from that, not from a role. The
	 * quickest way to check it is to stand where the supervisor stands.
	 */
	const impersonateAction = (userId: string, fullName: string): ActionMenuItem[] => {
		if (!administrator) return [];

		const isSelf = currentUser?.userId === userId;

		return [
			{
				label: "Log in as",
				icon: LogIn,
				disabled: isSelf,
				hint: isSelf ? "You are already signed in as yourself." : undefined,
				action: () => onImpersonate({ id: userId, fullName }),
			},
		];
	};

	/*
	 * Both of these are read off the chart we already hold, so the action can say why it is closed
	 * instead of letting a click come back as a 400 the person could not have anticipated. The
	 * backend stays the authority - this only spares them the round trip.
	 */
	const archiveBlocked = archiveBlockedReason(units, unit);
	const moveBlocked = isRoot ? rootCannotMoveMessage : null;

	return (
		<section className="org-structure-pane">
			<header className="org-structure-pane-header">
				<div>
					<h2>{unit.name}</h2>

					<p className="org-unit-member-meta">
						{orgUnitKinds[unit.kind]}
						{unit.isArchived ? " · archived" : ""}
					</p>
				</div>

				<ActionMenu
					ariaLabel={`Actions for ${unit.name}`}
					actions={[
						{ label: "Rename", icon: Pencil, action: onRename },
						{ label: "Add unit under this one", icon: Plus, action: onCreateChild },
						{
							label: "Move",
							icon: ArrowRightLeft,
							disabled: moveBlocked !== null,
							hint: moveBlocked ?? undefined,
							action: onMove,
						},
						{
							label: "Archive",
							icon: Archive,
							disabled: unit.isArchived || archiveBlocked !== null,
							hint: unit.isArchived ? alreadyArchivedMessage : (archiveBlocked ?? undefined),
							action: onArchive,
						},
					]}
				/>
			</header>

			<div className="org-unit-section">
				<div className="org-unit-section-header">
					<div>
						<h3>Head</h3>

						<p>
							Who answers for this unit. Leaving it empty is a shape, not a gap - then the head of
							the unit above answers for it.
						</p>
					</div>

					<div className="flex gap-2 shrink-0">
						<Button variant="secondary" icon={<UserRoundCog size={15} />} onClick={onAssignHead}>
							{unit.headUserId ? "Change" : "Assign"}
						</Button>

						{unit.headUserId ? (
							<Button variant="ghost" icon={<X size={15} />} onClick={onClearHead}>
								Clear
							</Button>
						) : null}
					</div>
				</div>

				{unit.headUserId ? (
					<div className="org-unit-member">
						<Avatar
							className="data-avatar"
							name={nameOf(head, "")}
							src={userAvatarUrl(unit.headUserId, avatarOf(unit.headUserId))}
						/>

						<div className="min-w-0 flex-1">
							<div className="org-unit-member-name">{nameOf(head, "Unknown person")}</div>
							<div className="org-unit-member-meta">{head?.email}</div>
						</div>

						{administrator ? (
							<ActionMenu
								ariaLabel={`Actions for ${nameOf(head, unit.headUserId)}`}
								actions={impersonateAction(unit.headUserId, nameOf(head, "this person"))}
							/>
						) : null}
					</div>
				) : (
					<p className="org-unit-empty">No head of its own.</p>
				)}
			</div>

			<div className="org-unit-section">
				<div className="org-unit-section-header">
					<div>
						<h3>People</h3>

						<p>Everybody who sits in this unit. One person belongs to one unit.</p>
					</div>

					<Button
						variant="secondary"
						icon={<Plus size={15} />}
						disabled={unit.isArchived}
						onClick={onAddMember}
					>
						Add person
					</Button>
				</div>

				{unit.members.length === 0 ? (
					<p className="org-unit-empty">Nobody here yet.</p>
				) : (
					<ul>
						{unit.members.map((member) => {
							const person = resolveUser(member.userId);
							const headsIt = unit.headUserId === member.userId;

							return (
								<li key={member.userId} className="org-unit-member">
									<Avatar
										className="data-avatar"
										name={nameOf(person, "")}
										src={userAvatarUrl(member.userId, avatarOf(member.userId))}
									/>

									<div className="min-w-0 flex-1">
										<div className="org-unit-member-name">{nameOf(person, "Unknown person")}</div>

										<div className="org-unit-member-meta">
											{member.title || person?.jobTitle || person?.email}
										</div>
									</div>

									<ActionMenu
										ariaLabel={`Actions for ${nameOf(person, member.userId)}`}
										actions={[
											{
												label: "Take out of this unit",
												icon: UserMinus,
												disabled: headsIt,
												hint: headsIt ? memberHeadsUnitMessage : undefined,
												action: () => onRemoveMember(member),
											},
											...impersonateAction(member.userId, nameOf(person, "this person")),
										]}
									/>
								</li>
							);
						})}
					</ul>
				)}
			</div>
		</section>
	);
}
