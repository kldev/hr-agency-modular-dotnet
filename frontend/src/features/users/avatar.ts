/**
 * Where to fetch somebody else's picture from. The path never changes, so the file id is what tells
 * the browser it is looking at a new picture rather than the one it already cached - the same trick
 * the profile page plays with the caller's own.
 *
 * Reading is open to everybody in the organization; only setting one is the administrator's job.
 */
export function userAvatarUrl(userId: string, avatarFileId: string | null | undefined) {
	return avatarFileId ? `/api/users/${userId}/avatar?v=${avatarFileId}` : null;
}
