import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { useRef, useState } from "react";
import { getUser, getUsers } from "#/api/endpoints";
import type { OrganizationRoleApi } from "#/api/models";
import {
	addTeamMemberServerFn,
	changeTeamMemberRoleServerFn,
	removeTeamMemberServerFn,
} from "#/features/teams/pages/hooks";
import { getFnOptions } from "#/server/axios";
import { changeUserRole, createUser, updateUser } from "@/api/endpoints";
import type {
	ChangeUserRoleRequest,
	CreateUserRequest,
	TeamInfo,
	TeamRole,
	UpdateUserRequest,
} from "@/api/models";
import { suggestionKeys, teamsKeys, usersKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

const PAGE_SIZE = 15;

export type UsersFilters = {
	search?: string;
	role?: OrganizationRoleApi;
	page?: number;
	pageSize?: number;
};

type MutationOptions = {
	onSuccess: () => void;
};

const getUsersliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: UsersFilters) => input)
	.handler(async ({ data }) => {
		return getUsers(
			{
				search: data.search ?? "",
				...(data.role ? { roles: [data.role] } : { roles: [] }),
				page: data.page,
				pageSize: data.pageSize,
			},
			await getFnOptions(),
		);
	});

const getUserServerFn = createServerFn({
	method: "GET",
})
	.validator((input: string) => input)
	.handler(async ({ data }) => {
		return getUser(data, await getFnOptions());
	});

const createUserServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { req: CreateUserRequest }) => input)
	.handler(async ({ data }) => {
		return createUser(data.req, await getFnOptions());
	});

const updateUserServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; req: UpdateUserRequest }) => input)
	.handler(async ({ data }) => {
		return updateUser(data.id, data.req, await getFnOptions());
	});

const changeUserRoleServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; req: ChangeUserRoleRequest }) => input)
	.handler(async ({ data }) => {
		return changeUserRole(data.id, data.req, await getFnOptions());
	});

export function useGetUsersSlice(filter: UsersFilters) {
	return useInfiniteQuery({
		queryKey: usersKeys.list(filter),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getUsersliceServerFn({
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

export function useGetUser(id: string) {
	return useQuery({
		queryKey: usersKeys.detail(id),
		queryFn: () => getUserServerFn({ data: id }),
		enabled: Boolean(id),
	});
}

/*
 * Team membership shows up on the user read model, so anything that touches a user invalidates the
 * teams key as well - otherwise a roster left open in another tab keeps the previous answer.
 */
function useUserMutation<TVariables, TResult>(
	mutationFn: (variables: TVariables) => Promise<TResult>,
	{ onSuccess }: MutationOptions,
) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn,

		onSuccess: async () => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: usersKeys.all });
			await queryClient.invalidateQueries({ queryKey: teamsKeys.all });
			await queryClient.invalidateQueries({ queryKey: suggestionKeys.all });

			onSuccess();
		},
	});

	return { mutation, waiting };
}

export function useCreateUser(options: MutationOptions) {
	return useUserMutation(
		({ request }: { request: CreateUserRequest }) => createUserServerFn({ data: { req: request } }),
		options,
	);
}

export function useUpdateUser(options: MutationOptions) {
	return useUserMutation(
		({ userId, request }: { userId: string; request: UpdateUserRequest }) =>
			updateUserServerFn({ data: { id: userId, req: request } }),
		options,
	);
}

export function useChangeUserRole(options: MutationOptions) {
	return useUserMutation(
		({ userId, request }: { userId: string; request: ChangeUserRoleRequest }) =>
			changeUserRoleServerFn({ data: { id: userId, req: request } }),
		options,
	);
}

export type ChangeTeamVariables = {
	userId: string;
	current: TeamInfo | null | undefined;
	teamId: string | null;
	role: TeamRole;
};

/**
 * Identity owns no path to a team - `UserProjection.Team` is a mirror kept in step by an integration
 * event - so changing where somebody sits means driving the Teams endpoints from here.
 *
 * Moving between two teams is the one case that takes two calls, and they are not one transaction.
 * The order is forced by the "one person, one team" reservation: adding before removing would bounce
 * off the reservation the old team still holds. If the add then fails, the person is left without a
 * team, and `detached` says so, because "something went wrong" would hide a change that did happen.
 */
export function useChangeUserTeam({ onSuccess }: MutationOptions) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();
	const [detached, setDetached] = useState(false);

	/*
	 * Sticky until a move finishes. The drawer holds the team the person was on when it opened, and
	 * a half-done move makes that snapshot a lie - they are already off it. Retrying against the
	 * stale snapshot would send a second remove, which the domain answers with a 404, and picking
	 * the old team again would look like "nothing to do" while they sit on no team.
	 */
	const detachedRef = useRef(false);

	const mutation = useMutation({
		mutationFn: async ({ userId, current: snapshot, teamId, role }: ChangeTeamVariables) => {
			const current = detachedRef.current ? null : snapshot;

			const staysOnTheSameTeam = current && teamId === current.id;

			if (staysOnTheSameTeam) {
				if (current.role === role) {
					return;
				}

				await changeTeamMemberRoleServerFn({
					data: { id: current.id, userId, req: { role } },
				});

				return;
			}

			if (current) {
				await removeTeamMemberServerFn({ data: { id: current.id, userId } });
			}

			if (!teamId) {
				return;
			}

			try {
				await addTeamMemberServerFn({ data: { id: teamId, req: { userId, role } } });
			} catch (error) {
				if (current) {
					detachedRef.current = true;
					setDetached(true);
				}

				throw error;
			}
		},

		onSuccess: async () => {
			detachedRef.current = false;
			setDetached(false);

			await wait();

			await queryClient.invalidateQueries({ queryKey: usersKeys.all });
			await queryClient.invalidateQueries({ queryKey: teamsKeys.all });
			await queryClient.invalidateQueries({ queryKey: suggestionKeys.all });

			onSuccess();
		},
	});

	return { mutation, waiting, detached };
}
