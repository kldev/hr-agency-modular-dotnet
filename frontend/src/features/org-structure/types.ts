import type { OrgUnitKind, OrgUnitRow, UserProjection } from "@/api/models";

/**
 * Board, department and section describe a box; they steer nothing. No rule in
 * `HrAgencySystem.Agency` reads `OrgUnitKind` - the shape of the tree is what carries the meaning,
 * which is why adding a level is a row rather than a branch.
 */
export const orgUnitKinds: Record<OrgUnitKind, string> = {
	Board: "Board",
	Department: "Department",
	Section: "Section",
};

/*
 * Mirrored from the handlers in `src/HrAgencySystem.Agency/Application/OrgUnits/`. Only the rules
 * that are true of the chart we already hold in memory are repeated here - the whole chart arrives
 * in one read, so "is this unit inside its own subtree" is a local fact, not a query.
 *
 * Deliberately not mirrored, because both need the write side to answer: whether a sibling already
 * carries a name, and whether somebody already belongs to another unit. Those come back as a named
 * 400 and are rendered by `ApiError`; guessing them here would risk two contradictory messages.
 */

/** `OrgUnitName.RequiredMessage` */
export const unitNameRequiredMessage = "The unit needs a name.";

/** `OrgUnitName.MaxLengthMessage` */
export const unitNameTooLongMessage = "The unit name cannot exceed 100 characters.";

export const unitNameMaxLength = 100;

/** `MoveOrgUnitHandler.RootCannotMoveMessage` */
export const rootCannotMoveMessage = "The top unit has nothing to hang under.";

/** `MoveOrgUnitHandler.IntoItselfMessage` */
export const moveIntoItselfMessage = "A unit cannot be moved under itself.";

/** `MoveOrgUnitHandler.IntoOwnSubtreeMessage` */
export const moveIntoOwnSubtreeMessage =
	"A unit cannot be moved under one of its own units - that would leave a loop with no top.";

/** `CreateOrgUnitHandler.ParentArchivedMessage` */
export const parentArchivedMessage = "That parent unit is archived, so nothing new goes under it.";

/** `ArchiveOrgUnitHandler.RootCannotBeArchivedMessage` */
export const rootCannotBeArchivedMessage =
	"The top unit stays. Archiving it would leave the chart with no top.";

/** `ArchiveOrgUnitHandler.AlreadyArchivedMessage` */
export const alreadyArchivedMessage = "This unit is already archived.";

/** `ArchiveOrgUnitHandler.StillHasPeopleMessage` */
export const unitStillHasPeopleMessage =
	"This unit still has people in it. Move them somewhere else first.";

/** `ArchiveOrgUnitHandler.StillHasUnitsMessage` */
export const unitStillHasUnitsMessage =
	"This unit still has units under it. Move or archive those first.";

/** `RemoveOrgUnitMemberHandler.IsTheHeadMessage` */
export const memberHeadsUnitMessage =
	"This person heads this unit. Name another head, or clear it, before taking them out.";

/** `AssignOrgUnitHeadHandler.NotAMemberMessage` */
export const headMustBeAMemberMessage =
	"The head of a unit is one of its people. Put them in the unit first.";

/** A unit with its children resolved - everything the tree renderer needs, and nothing more. */
export type OrgUnitNode = OrgUnitRow & {
	children: OrgUnitNode[];
	depth: number;
};

const byName = (left: OrgUnitRow, right: OrgUnitRow) => left.name.localeCompare(right.name);

/**
 * The flat projection turned into the tree it describes. Archived units are filtered out *before*
 * the tree is built rather than skipped while rendering, so the toggle never leaves a chevron that
 * expands into nothing.
 */
export function buildOrgUnitTree(units: OrgUnitRow[], includeArchived: boolean): OrgUnitNode[] {
	const visible = includeArchived ? units : units.filter((unit) => !unit.isArchived);
	const visibleIds = new Set(visible.map((unit) => unit.unitId));

	const childrenOf = new Map<string, OrgUnitRow[]>();

	for (const unit of visible) {
		if (unit.parentId === null) {
			continue;
		}

		const siblings = childrenOf.get(unit.parentId) ?? [];

		siblings.push(unit);
		childrenOf.set(unit.parentId, siblings);
	}

	const build = (unit: OrgUnitRow, depth: number): OrgUnitNode => ({
		...unit,
		depth,
		children: [...(childrenOf.get(unit.unitId) ?? [])]
			.sort(byName)
			.map((child) => build(child, depth + 1)),
	});

	/*
	 * A unit whose parent is hidden counts as a root here instead of being dropped. With archived
	 * units off, a live section under an archived department would otherwise disappear from the
	 * chart altogether, which reads as lost data rather than as a filter.
	 */
	return visible
		.filter((unit) => unit.parentId === null || !visibleIds.has(unit.parentId))
		.sort(byName)
		.map((unit) => build(unit, 0));
}

export function flattenOrgUnitTree(nodes: OrgUnitNode[]): OrgUnitNode[] {
	return nodes.flatMap((node) => [node, ...flattenOrgUnitTree(node.children)]);
}

export function findRootUnit(units: OrgUnitRow[]): OrgUnitRow | undefined {
	return units.find((unit) => unit.parentId === null);
}

/** Which unit somebody sits in. One or none, never two - the domain reserves that. */
export function unitOfUser(units: OrgUnitRow[], userId: string): OrgUnitRow | undefined {
	return units.find((unit) => unit.members.some((member) => member.userId === userId));
}

export function unitByMemberId(units: OrgUnitRow[]): Map<string, OrgUnitRow> {
	const byMember = new Map<string, OrgUnitRow>();

	for (const unit of units) {
		for (const member of unit.members) {
			byMember.set(member.userId, unit);
		}
	}

	return byMember;
}

/** Mirrors `SupervisorPolicy.Descendants`, with the unit itself included - both callers want it. */
export function subtreeIds(units: OrgUnitRow[], unitId: string): Set<string> {
	const found = new Set<string>([unitId]);
	const queue: string[] = [unitId];

	while (queue.length > 0) {
		const parentId = queue.shift() as string;

		for (const unit of units) {
			if (unit.parentId === parentId && !found.has(unit.unitId)) {
				found.add(unit.unitId);
				queue.push(unit.unitId);
			}
		}
	}

	return found;
}

export type MoveTarget = {
	unit: OrgUnitRow;
	depth: number;
	disabledReason: string | null;
};

/**
 * Every unit as a possible new parent, in tree order, with the ones the domain would refuse carrying
 * the reason it would give. Offered disabled rather than hidden: "you cannot put a department under
 * its own section" is worth reading once, and a silently missing row teaches nothing.
 */
export function moveTargets(units: OrgUnitRow[], moved: OrgUnitRow): MoveTarget[] {
	const ownSubtree = subtreeIds(units, moved.unitId);

	return flattenOrgUnitTree(buildOrgUnitTree(units, true)).map((node) => ({
		unit: node,
		depth: node.depth,
		disabledReason: moveTargetReason(node, moved, ownSubtree),
	}));
}

function moveTargetReason(
	target: OrgUnitRow,
	moved: OrgUnitRow,
	ownSubtree: Set<string>,
): string | null {
	if (target.unitId === moved.unitId) {
		return moveIntoItselfMessage;
	}

	if (ownSubtree.has(target.unitId)) {
		return moveIntoOwnSubtreeMessage;
	}

	if (target.isArchived) {
		return parentArchivedMessage;
	}

	if (target.unitId === moved.parentId) {
		return "This is where the unit already hangs.";
	}

	return null;
}

/**
 * The people a unit may name as its head, which is exactly the people already in it - the domain
 * refuses anybody else. Resolved to names here so the drawer never sees a bare id.
 */
export function headCandidates(
	unit: OrgUnitRow,
	resolveUser: (userId: string) => UserProjection | undefined,
): { userId: string; name: string; email: string }[] {
	return unit.members.map((member) => {
		const person = resolveUser(member.userId);

		return {
			userId: member.userId,
			name: nameOf(person, member.userId),
			email: person?.email ?? "",
		};
	});
}

/**
 * Why this unit cannot be archived yet, or null when it can. Checked in the handler's own order, so
 * a unit that is both full and a parent reports the same thing the backend would.
 */
export function archiveBlockedReason(units: OrgUnitRow[], unit: OrgUnitRow): string | null {
	if (unit.parentId === null) {
		return rootCannotBeArchivedMessage;
	}

	if (unit.members.length > 0) {
		return unitStillHasPeopleMessage;
	}

	const hasLiveChildren = units.some(
		(candidate) => candidate.parentId === unit.unitId && !candidate.isArchived,
	);

	return hasLiveChildren ? unitStillHasUnitsMessage : null;
}

/**
 * How a person is written on the chart. `fullName` is computed by the backend and optional, so the
 * two halves stand in for it rather than letting a row read "undefined undefined".
 */
export function nameOf(user: UserProjection | undefined, fallback: string): string {
	if (!user) {
		return fallback;
	}

	return user.fullName ?? `${user.firstName} ${user.lastName}`;
}

/**
 * The sentence the backend refused with. A 400 arrives as `BadRequestDetails` rather than as an
 * `Error` - the axios mutator unwraps it - so the named business rule is in `title`.
 */
export function refusalMessage(error: unknown, fallback: string): string {
	if (error && typeof error === "object") {
		const details = error as { title?: unknown; detail?: unknown };

		if (typeof details.title === "string" && details.title.length > 0) {
			return details.title;
		}

		if (typeof details.detail === "string" && details.detail.length > 0) {
			return details.detail;
		}
	}

	return fallback;
}
