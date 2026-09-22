import { Network, Plus } from "lucide-react";
import type React from "react";
import { useMemo, useRef, useState } from "react";
import { toast } from "sonner";
import { Route } from "#/routes/app/org-structure";
import type { OrgUnitMember, OrgUnitRow } from "@/api/models";
import { Page } from "@/components/layout";
import { Button, ConfirmDialog, EmptyState, Toggle } from "@/components/ui";
import {
	AddOrgUnitMemberDrawer,
	type AddOrgUnitMemberFormCommand,
	AssignOrgUnitHeadDrawer,
	type AssignOrgUnitHeadFormCommand,
	CreateOrgUnitDrawer,
	type CreateOrgUnitFormCommand,
	MoveOrgUnitDrawer,
	type MoveOrgUnitFormCommand,
	RenameOrgUnitDrawer,
	type RenameOrgUnitFormCommand,
} from "../drawers";
import {
	buildOrgUnitTree,
	findRootUnit,
	headCandidates,
	moveTargets,
	nameOf,
	refusalMessage,
} from "../types";
import { OrgUnitTree } from "./components/OrgUnitTree";
import { UnitPanel } from "./components/UnitPanel";
import {
	useArchiveOrgUnit,
	useClearOrgUnitHead,
	useGetOrgStructure,
	useRemoveOrgUnitMember,
} from "./hooks";
import "./org-structure.css";
import { ImpersonateUserDialog } from "#/features/users/components";
import type { ImpersonateUserFormCommand } from "#/features/users/components/form";
import { useAllOrganizationUsers, useUserAvatars } from "#/features/users/pages/hooks";

/**
 * The company's own chart: who sits where, and - through that - who answers for whom. The supervisor
 * is never edited here because it is never stored; it follows from the shape of this tree.
 */
const OrgStructurePage: React.FC = () => {
	const impersonateRef = useRef<ImpersonateUserFormCommand>(null);
	const { avatarOf } = useUserAvatars();
	const navigate = Route.useNavigate();
	const search = Route.useSearch();

	const createRef = useRef<CreateOrgUnitFormCommand>(null);
	const renameRef = useRef<RenameOrgUnitFormCommand>(null);
	const moveRef = useRef<MoveOrgUnitFormCommand>(null);
	const headRef = useRef<AssignOrgUnitHeadFormCommand>(null);
	const memberRef = useRef<AddOrgUnitMemberFormCommand>(null);

	const [pendingArchive, setPendingArchive] = useState<OrgUnitRow | null>(null);
	const [pendingRemoval, setPendingRemoval] = useState<{
		unit: OrgUnitRow;
		member: OrgUnitMember;
	} | null>(null);

	const query = useGetOrgStructure();

	/*
	 * The chart carries user ids and no names, so the whole directory comes along to resolve them.
	 * One request for every head and member row on the page - the alternative is one per id.
	 */
	const directory = useAllOrganizationUsers();

	const units = useMemo(() => query.data?.units ?? [], [query.data]);

	const people = useMemo(
		() => new Map((directory.data?.content ?? []).map((user) => [user.id, user])),
		[directory.data],
	);

	const resolveUser = (userId: string) => people.get(userId);

	const includeArchived = search.includeArchived ?? false;

	const tree = useMemo(() => buildOrgUnitTree(units, includeArchived), [units, includeArchived]);

	const requested = units.find((unit) => unit.unitId === search.unit);

	/*
	 * The panel only ever shows a unit the tree beside it also shows. A unit the archived filter
	 * hides falls back to the top of the chart, exactly as an id that no longer exists does - which
	 * is also what puts the selection somewhere sensible the moment the open unit is archived.
	 */
	const selected =
		(requested && (includeArchived || !requested.isArchived) ? requested : undefined) ??
		findRootUnit(units) ??
		units[0] ??
		null;

	const refresh = () => {
		void query.refetch();
	};

	const select = (unitId: string) => {
		void navigate({ search: (previous) => ({ ...previous, unit: unitId }) });
	};

	/*
	 * These three are the writes without a form behind them, so there is no `ApiError` banner to put
	 * a refusal in. The preconditions are mirrored onto the actions themselves, which makes a failure
	 * unlikely - but a dialog that just closes on one would be worse than the refusal it swallowed.
	 */
	const reportRefusal = (error: unknown) =>
		toast.error(refusalMessage(error, "The change was refused."));

	const archive = useArchiveOrgUnit({
		onSuccess: () => {
			setPendingArchive(null);
		},
		onError: reportRefusal,
	});

	const clearHead = useClearOrgUnitHead({
		onError: reportRefusal,
	});

	const removeMember = useRemoveOrgUnitMember({
		onSuccess: () => {
			setPendingRemoval(null);
		},
		onError: reportRefusal,
	});

	const isEmpty = query.isFetched && units.length === 0;

	return (
		<>
			<Page
				title="Structure"
				description="The company's own chart: the departments, who heads them and who sits in them."
				onRefresh={refresh}
				loading={query.isPending}
				isEmpty={isEmpty}
				emptyState={
					<EmptyState
						title="No structure yet"
						description="The chart starts with one top unit - the board - and everything else hangs underneath it."
					>
						<Network size={24} />
					</EmptyState>
				}
				headerAddon={
					isEmpty ? (
						<Button
							variant="primary"
							icon={<Plus size={15} />}
							onClick={() => createRef.current?.create(null)}
						>
							Create the top unit
						</Button>
					) : null
				}
			>
				{units.length > 0 ? (
					<div className="org-structure-layout">
						<section className="org-structure-pane">
							<header className="org-structure-pane-header">
								<h2>Chart</h2>

								<span className="toolbar-toggle">
									<Toggle
										id="org-structure-include-archived"
										checked={includeArchived}
										onChange={(event) =>
											void navigate({
												search: (previous) => ({
													...previous,
													includeArchived: event.target.checked ? true : undefined,
												}),
											})
										}
									/>

									<label htmlFor="org-structure-include-archived">Include archived</label>
								</span>
							</header>

							<OrgUnitTree
								nodes={tree}
								selectedId={selected?.unitId ?? null}
								onSelect={select}
								resolveUser={resolveUser}
							/>
						</section>

						{selected ? (
							<UnitPanel
								unit={selected}
								units={units}
								resolveUser={resolveUser}
								onRename={() => renameRef.current?.rename(selected)}
								onCreateChild={() => createRef.current?.create(selected)}
								onMove={() => moveRef.current?.move(selected, moveTargets(units, selected))}
								onArchive={() => setPendingArchive(selected)}
								onAssignHead={() =>
									headRef.current?.assign(selected, headCandidates(selected, resolveUser))
								}
								onClearHead={() => clearHead.mutation.mutate({ unitId: selected.unitId })}
								onAddMember={() => memberRef.current?.add(selected)}
								onRemoveMember={(member) => setPendingRemoval({ unit: selected, member })}
								onImpersonate={(user) => impersonateRef.current?.impersonate(user)}
								avatarOf={avatarOf}
							/>
						) : null}
					</div>
				) : null}
			</Page>

			<ConfirmDialog
				open={pendingArchive !== null}
				title="Archive unit"
				description={`${pendingArchive?.name ?? "This unit"} disappears from the chart but stays on everything already approved through it. A dissolved department does not come back.`}
				confirmLabel="Archive"
				loading={archive.mutation.isPending}
				onClose={() => setPendingArchive(null)}
				onConfirm={() => {
					if (pendingArchive) {
						archive.mutation.mutate({ unitId: pendingArchive.unitId });
					}
				}}
			/>

			<ConfirmDialog
				open={pendingRemoval !== null}
				title="Take out of the unit"
				description={`${nameOf(
					pendingRemoval ? resolveUser(pendingRemoval.member.userId) : undefined,
					"This person",
				)} will sit in no unit, so nobody will answer for them until they are put somewhere else.`}
				confirmLabel="Take out"
				loading={removeMember.mutation.isPending}
				onClose={() => setPendingRemoval(null)}
				onConfirm={() => {
					if (pendingRemoval) {
						removeMember.mutation.mutate({
							unitId: pendingRemoval.unit.unitId,
							userId: pendingRemoval.member.userId,
						});
					}
				}}
			/>

			<CreateOrgUnitDrawer ref={createRef} onSuccess={select} />
			<RenameOrgUnitDrawer ref={renameRef} onSuccess={refresh} />
			<MoveOrgUnitDrawer ref={moveRef} onSuccess={refresh} />
			<AssignOrgUnitHeadDrawer ref={headRef} onSuccess={refresh} />
			<AddOrgUnitMemberDrawer ref={memberRef} onSuccess={refresh} />
			<ImpersonateUserDialog ref={impersonateRef} />
		</>
	);
};

export default OrgStructurePage;
