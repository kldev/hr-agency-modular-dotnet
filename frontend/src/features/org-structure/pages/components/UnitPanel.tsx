import { Archive, ArrowRightLeft, Pencil, Plus, UserMinus, UserRoundCog, X } from "lucide-react";
import type { OrgUnitMember, OrgUnitRow, UserProjection } from "@/api/models";
import { Button, ItemMark } from "@/components/ui";
import { ActionMenu } from "@/components/ui/ActionMenu";
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
}: UnitPanelProps) {
	const isRoot = unit.parentId === null;
	const head = unit.headUserId ? resolveUser(unit.headUserId) : undefined;

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

					<div className="flex gap-2">
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
						<ItemMark name={nameOf(head, "")} />

						<div className="min-w-0 flex-1">
							<div className="org-unit-member-name">{nameOf(head, "Unknown person")}</div>
							<div className="org-unit-member-meta">{head?.email}</div>
						</div>
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
									<ItemMark name={nameOf(person, "")} />

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
