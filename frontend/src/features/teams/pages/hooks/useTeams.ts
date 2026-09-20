import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import {
	addTeamMember,
	changeTeamMemberRole,
	createTeam,
	getTeam,
	getTeams,
	removeTeamMember,
	renameTeam,
} from "@/api/endpoints";
import type {
	AddTeamMemberRequest,
	ChangeTeamMemberRoleRequest,
	CreateTeamRequest,
	RenameTeamRequest,
} from "@/api/models";
import { teamsKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

const PAGE_SIZE = 15;

export type TeamsFilters = {
	search?: string;
	userId?: string;
	page?: number;
	pageSize?: number;
};

type MutationOptions = {
	onSuccess: () => void;
};

const getTeamsSliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: TeamsFilters) => input)
	.handler(async ({ data }) => {
		return getTeams(
			{
				search: data.search ?? "",
				...(data.userId ? { userId: data.userId } : {}),
				page: data.page,
				pageSize: data.pageSize,
			},
			await getFnOptions(),
		);
	});

const getTeamServerFn = createServerFn({
	method: "GET",
})
	.validator((input: string) => input)
	.handler(async ({ data }) => {
		return getTeam(data, await getFnOptions());
	});

const createTeamServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { req: CreateTeamRequest }) => input)
	.handler(async ({ data }) => {
		return createTeam(data.req, await getFnOptions());
	});

const renameTeamServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; req: RenameTeamRequest }) => input)
	.handler(async ({ data }) => {
		return renameTeam(data.id, data.req, await getFnOptions());
	});

const addTeamMemberServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; req: AddTeamMemberRequest }) => input)
	.handler(async ({ data }) => {
		return addTeamMember(data.id, data.req, await getFnOptions());
	});

const removeTeamMemberServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; userId: string }) => input)
	.handler(async ({ data }) => {
		return removeTeamMember(data.id, data.userId, await getFnOptions());
	});

const changeTeamMemberRoleServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; userId: string; req: ChangeTeamMemberRoleRequest }) => input)
	.handler(async ({ data }) => {
		return changeTeamMemberRole(data.id, data.userId, data.req, await getFnOptions());
	});

export function useGetTeamsSlice(filter: TeamsFilters) {
	return useInfiniteQuery({
		queryKey: teamsKeys.list(filter),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getTeamsSliceServerFn({
				data: {
					...filter,
					page: pageParam,
					pageSize: PAGE_SIZE,
				},
			}),

		getNextPageParam: (lastPage, _pages, lastPageParam) => {
			return lastPage.hasMore ? lastPageParam + 1 : undefined;
		},
	});
}

export function useGetTeam(id: string) {
	return useQuery({
		queryKey: teamsKeys.detail(id),
		queryFn: () => getTeamServerFn({ data: id }),
		enabled: Boolean(id),
	});
}

/*
 * Every team mutation invalidates the whole teams key. Membership moves a person between two teams,
 * and a rename fans out to the user read model, so narrowing the invalidation to one id would leave
 * the other side of the change stale on screen.
 */
function useTeamMutation<TVariables, TResult>(
	mutationFn: (variables: TVariables) => Promise<TResult>,
	{ onSuccess }: MutationOptions,
) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn,

		onSuccess: async () => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: teamsKeys.all });

			onSuccess();
		},
	});

	return { mutation, waiting };
}

export function useCreateTeam(options: MutationOptions) {
	return useTeamMutation(
		({ request }: { request: CreateTeamRequest }) => createTeamServerFn({ data: { req: request } }),
		options,
	);
}

export function useRenameTeam(options: MutationOptions) {
	return useTeamMutation(
		({ teamId, request }: { teamId: string; request: RenameTeamRequest }) =>
			renameTeamServerFn({ data: { id: teamId, req: request } }),
		options,
	);
}

export function useAddTeamMember(options: MutationOptions) {
	return useTeamMutation(
		({ teamId, request }: { teamId: string; request: AddTeamMemberRequest }) =>
			addTeamMemberServerFn({ data: { id: teamId, req: request } }),
		options,
	);
}

export function useRemoveTeamMember(options: MutationOptions) {
	return useTeamMutation(
		({ teamId, userId }: { teamId: string; userId: string }) =>
			removeTeamMemberServerFn({ data: { id: teamId, userId } }),
		options,
	);
}

export function useChangeTeamMemberRole(options: MutationOptions) {
	return useTeamMutation(
		({
			teamId,
			userId,
			request,
		}: {
			teamId: string;
			userId: string;
			request: ChangeTeamMemberRoleRequest;
		}) => changeTeamMemberRoleServerFn({ data: { id: teamId, userId, req: request } }),
		options,
	);
}
