import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { useState } from "react";
import {
	changeOwnPassword,
	getOwnProfile,
	removeOwnAvatar,
	updateOwnProfile,
	uploadOwnAvatar,
} from "#/api/endpoints";
import type { ChangePasswordRequest, UpdateOwnProfileRequest } from "#/api/models";
import { profileKeys, suggestionKeys, usersKeys } from "#/api/query-keys";
import { useProjectionWait } from "#/hooks";
import { getFnOptions } from "#/server/axios";

/**
 * Checked here as well as on the backend. Not as a gate - the client is not one - but so that a
 * picture that was never going to be accepted fails while the file dialog is still fresh in mind
 * rather than after a trip to the server.
 */
export const MAX_AVATAR_SIZE_BYTES = 512 * 1024;

export const ALLOWED_AVATAR_TYPES = ["image/png", "image/jpeg"];

export const AVATAR_ACCEPT = ".png,.jpg,.jpeg";

/** The picture lives at a fixed url, so the file id is what tells the browser it is a new one. */
export function avatarUrl(avatarFileId: string | null | undefined) {
	return avatarFileId ? `/api/users/me/avatar?v=${avatarFileId}` : null;
}

type MutationOptions = {
	onSuccess: () => void;
};

const getOwnProfileServerFn = createServerFn({ method: "GET" }).handler(async () => {
	return getOwnProfile(await getFnOptions());
});

const updateOwnProfileServerFn = createServerFn({ method: "POST" })
	.validator((input: UpdateOwnProfileRequest) => input)
	.handler(async ({ data }) => {
		return updateOwnProfile(data, await getFnOptions());
	});

const removeOwnAvatarServerFn = createServerFn({ method: "POST" }).handler(async () => {
	return removeOwnAvatar(await getFnOptions());
});

const changeOwnPasswordServerFn = createServerFn({ method: "POST" })
	.validator((input: ChangePasswordRequest) => input)
	.handler(async ({ data }) => {
		return changeOwnPassword(data, await getFnOptions());
	});

export function useGetOwnProfile() {
	return useQuery({
		queryKey: profileKeys.me(),
		queryFn: () => getOwnProfileServerFn(),
	});
}

/**
 * The name and the job title come off `UserProjection`, so an edit has to wait out the daemon before
 * anything reads them back. The avatar does not - it is a plain document - which is why the picture
 * mutations below invalidate without waiting.
 */
export function useUpdateOwnProfile({ onSuccess }: MutationOptions) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: (request: UpdateOwnProfileRequest) => updateOwnProfileServerFn({ data: request }),

		onSuccess: async () => {
			await wait();

			await invalidate(queryClient);

			onSuccess();
		},
	});

	return { mutation, waiting };
}

/*
 * The one call here that does not go through a server function. A progress bar needs the browser's
 * own upload events, and a server function would only start sending to the API once the whole file
 * had already reached the frontend server. The request goes to `/api/...`, where the proxy attaches
 * the bearer token, so the browser still never holds a credential.
 */
export function useUploadOwnAvatar({ onSuccess }: MutationOptions) {
	const queryClient = useQueryClient();
	const [progress, setProgress] = useState(0);

	const mutation = useMutation({
		mutationFn: (file: File) =>
			uploadOwnAvatar(
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

export function useRemoveOwnAvatar({ onSuccess }: MutationOptions) {
	const queryClient = useQueryClient();

	return useMutation({
		mutationFn: () => removeOwnAvatarServerFn(),

		onSuccess: async () => {
			await invalidate(queryClient);

			onSuccess();
		},
	});
}

/**
 * Changing the password ends every session, this one included - the backend revokes the whole family
 * of refresh tokens. Nothing is invalidated on success, because there is nothing left to show; the
 * caller signs out instead.
 */
export function useChangeOwnPassword({ onSuccess }: MutationOptions) {
	return useMutation({
		mutationFn: (request: ChangePasswordRequest) => changeOwnPasswordServerFn({ data: request }),
		onSuccess,
	});
}

async function invalidate(queryClient: ReturnType<typeof useQueryClient>) {
	await queryClient.invalidateQueries({ queryKey: profileKeys.all });

	// The same person appears in the administrative user list and in every picker that suggests one.
	await queryClient.invalidateQueries({ queryKey: usersKeys.all });
	await queryClient.invalidateQueries({ queryKey: suggestionKeys.all });
}
