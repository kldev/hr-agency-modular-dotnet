import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import {
	addOrgUnitMember,
	archiveOrgUnit,
	assignOrgUnitHead,
	clearOrgUnitHead,
	createOrgUnit,
	moveOrgUnit,
	removeOrgUnitMember,
	renameOrgUnit,
} from "@/api/endpoints";
import type {
	AddOrgUnitMemberRequest,
	AssignOrgUnitHeadRequest,
	CreateOrgUnitRequest,
	MoveOrgUnitRequest,
	OrgUnitArchived,
	OrgUnitCreated,
	OrgUnitHeadAssigned,
	OrgUnitHeadCleared,
	OrgUnitMemberAdded,
	OrgUnitMemberRemoved,
	OrgUnitMoved,
	OrgUnitRenamed,
	RenameOrgUnitRequest,
} from "@/api/models";
import { orgStructureKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

/*
 * `onSuccess` is handed the command's own event, because one caller needs it: creating a unit
 * selects the unit that was just created, and `mutation.data` is not settled yet at this point.
 */
type MutationOptions<TResult = unknown> = {
	onSuccess: (result: TResult) => void;

	/*
	 * Only for the writes driven from a confirmation dialog or a bare button. A drawer renders the
	 * refusal inline through `ApiError`, and would say it twice if it also took this.
	 */
	onError?: (error: unknown) => void;
};

const createOrgUnitServerFn = createServerFn({ method: "POST" })
	.validator((input: { req: CreateOrgUnitRequest }) => input)
	.handler(async ({ data }) => createOrgUnit(data.req, await getFnOptions()));

const renameOrgUnitServerFn = createServerFn({ method: "POST" })
	.validator((input: { unitId: string; req: RenameOrgUnitRequest }) => input)
	.handler(async ({ data }) => renameOrgUnit(data.unitId, data.req, await getFnOptions()));

const moveOrgUnitServerFn = createServerFn({ method: "POST" })
	.validator((input: { unitId: string; req: MoveOrgUnitRequest }) => input)
	.handler(async ({ data }) => moveOrgUnit(data.unitId, data.req, await getFnOptions()));

const archiveOrgUnitServerFn = createServerFn({ method: "POST" })
	.validator((input: string) => input)
	.handler(async ({ data }) => archiveOrgUnit(data, await getFnOptions()));

const assignOrgUnitHeadServerFn = createServerFn({ method: "POST" })
	.validator((input: { unitId: string; req: AssignOrgUnitHeadRequest }) => input)
	.handler(async ({ data }) => assignOrgUnitHead(data.unitId, data.req, await getFnOptions()));

const clearOrgUnitHeadServerFn = createServerFn({ method: "POST" })
	.validator((input: string) => input)
	.handler(async ({ data }) => clearOrgUnitHead(data, await getFnOptions()));

const addOrgUnitMemberServerFn = createServerFn({ method: "POST" })
	.validator((input: { unitId: string; req: AddOrgUnitMemberRequest }) => input)
	.handler(async ({ data }) => addOrgUnitMember(data.unitId, data.req, await getFnOptions()));

const removeOrgUnitMemberServerFn = createServerFn({ method: "POST" })
	.validator((input: { unitId: string; userId: string }) => input)
	.handler(async ({ data }) => removeOrgUnitMember(data.unitId, data.userId, await getFnOptions()));

/*
 * Every write invalidates the whole org-structure key, and waits first. The wait is not decoration:
 * `OrgStructureProjection` is rebuilt by an async daemon, so a refetch fired the moment the command
 * returns shows the chart from before the change.
 *
 * The invalidation is the whole key rather than just the chart because the supervisor and
 * subordinate lookups hang under it - moving one person changes who answers for them, and those
 * two queries are the only place that shows.
 *
 * `usersKeys` is deliberately left alone. Team membership is mirrored onto `UserProjection.team`, so
 * `useChangeUserTeam` has to invalidate both sides; a unit is not mirrored onto the user at all, so
 * nothing on that record can go stale. The unit column and the rows on the user card read the chart.
 */
function useOrgStructureMutation<TVariables, TResult>(
	mutationFn: (variables: TVariables) => Promise<TResult>,
	{ onSuccess, onError }: MutationOptions<TResult>,
) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn,

		onSuccess: async (result) => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: orgStructureKeys.all });

			onSuccess(result);
		},

		onError,
	});

	return { mutation, waiting };
}

export function useCreateOrgUnit(options: MutationOptions<OrgUnitCreated>) {
	return useOrgStructureMutation(
		({ request }: { request: CreateOrgUnitRequest }) =>
			createOrgUnitServerFn({ data: { req: request } }),
		options,
	);
}

export function useRenameOrgUnit(options: MutationOptions<OrgUnitRenamed>) {
	return useOrgStructureMutation(
		({ unitId, request }: { unitId: string; request: RenameOrgUnitRequest }) =>
			renameOrgUnitServerFn({ data: { unitId, req: request } }),
		options,
	);
}

export function useMoveOrgUnit(options: MutationOptions<OrgUnitMoved>) {
	return useOrgStructureMutation(
		({ unitId, request }: { unitId: string; request: MoveOrgUnitRequest }) =>
			moveOrgUnitServerFn({ data: { unitId, req: request } }),
		options,
	);
}

export function useArchiveOrgUnit(options: MutationOptions<OrgUnitArchived>) {
	return useOrgStructureMutation(
		({ unitId }: { unitId: string }) => archiveOrgUnitServerFn({ data: unitId }),
		options,
	);
}

export function useAssignOrgUnitHead(options: MutationOptions<OrgUnitHeadAssigned>) {
	return useOrgStructureMutation(
		({ unitId, request }: { unitId: string; request: AssignOrgUnitHeadRequest }) =>
			assignOrgUnitHeadServerFn({ data: { unitId, req: request } }),
		options,
	);
}

export function useClearOrgUnitHead(options: MutationOptions<OrgUnitHeadCleared>) {
	return useOrgStructureMutation(
		({ unitId }: { unitId: string }) => clearOrgUnitHeadServerFn({ data: unitId }),
		options,
	);
}

export function useAddOrgUnitMember(options: MutationOptions<OrgUnitMemberAdded>) {
	return useOrgStructureMutation(
		({ unitId, request }: { unitId: string; request: AddOrgUnitMemberRequest }) =>
			addOrgUnitMemberServerFn({ data: { unitId, req: request } }),
		options,
	);
}

export function useRemoveOrgUnitMember(options: MutationOptions<OrgUnitMemberRemoved>) {
	return useOrgStructureMutation(
		({ unitId, userId }: { unitId: string; userId: string }) =>
			removeOrgUnitMemberServerFn({ data: { unitId, userId } }),
		options,
	);
}
