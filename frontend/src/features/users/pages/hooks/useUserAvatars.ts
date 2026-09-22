import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { useMemo, useState } from "react";
import { getUserAvatars, removeUserAvatar, uploadUserAvatar } from "#/api/endpoints";
import { profileKeys, suggestionKeys, usersKeys } from "#/api/query-keys";
import { getFnOptions } from "#/server/axios";

type MutationOptions = {
	onSuccess: () => void;
};

const getUserAvatarsServerFn = createServerFn({ method: "GET" }).handler(async () => {
	return getUserAvatars(await getFnOptions());
});

const removeUserAvatarServerFn = createServerFn({ method: "POST" })
	.validator((userId: string) => userId)
	.handler(async ({ data }) => {
		return removeUserAvatar(data, await getFnOptions());
	});

/**
 * Who in the organization has a picture, as one read. Deliberately not a field on the user rows:
 * the picture is a plain document while the rows come off a projection built from events, and a
 * list would otherwise have to ask per person and treat most of the answers as a 404.
 *
 * Returns a lookup rather than the array, because every caller wants "the file id for this person".
 */
export function useUserAvatars() {
	const query = useQuery({
		queryKey: usersKeys.avatars(),
		queryFn: () => getUserAvatarsServerFn(),
	});

	const byUserId = useMemo(() => {
		const map = new Map<string, string>();

		for (const entry of query.data ?? []) {
			map.set(entry.userId, entry.avatarFileId);
		}

		return map;
	}, [query.data]);

	return {
		query,
		avatarOf: (userId: string) => byUserId.get(userId) ?? null,
	};
}

/*
 * Like the profile's own upload, this one call skips the server function: a progress bar needs the
 * browser's upload events, and a server function would only start sending to the API once the whole
 * file had already arrived at the frontend server. It goes to `/api/...`, where the proxy attaches
 * the bearer, so the browser still never holds a credential.
 */
export function useUploadUserAvatar({ onSuccess }: MutationOptions) {
	const queryClient = useQueryClient();
	const [progress, setProgress] = useState(0);

	const mutation = useMutation({
		mutationFn: ({ userId, file }: { userId: string; file: File }) =>
			uploadUserAvatar(
				userId,
				{ file },
				{
					onUploadProgress: (event) => {
						if (!event.total) return;

						setProgress(Math.round((event.loaded / event.total) * 100));
					},
				},
			),

		onSuccess: async () => {
			await invalidate(queryClient);

			setProgress(0);
			onSuccess();
		},

		onError: () => {
			setProgress(0);
		},
	});

	return { mutation, progress };
}

export function useRemoveUserAvatar({ onSuccess }: MutationOptions) {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: (userId: string) => removeUserAvatarServerFn({ data: userId }),

		onSuccess: async () => {
			await invalidate(queryClient);

			onSuccess();
		},
	});
}

/**
 * No `useProjectionWait` anywhere here: the picture is a document, so it is readable the moment the
 * call answers. `profileKeys` is in the list because an administrator may well be fixing their own
 * row, and the top bar reads the picture from there.
 */
async function invalidate(queryClient: ReturnType<typeof useQueryClient>) {
	await queryClient.invalidateQueries({ queryKey: usersKeys.all });
	await queryClient.invalidateQueries({ queryKey: profileKeys.all });
	await queryClient.invalidateQueries({ queryKey: suggestionKeys.all });
}
