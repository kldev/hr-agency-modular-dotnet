import { useMutation } from "@tanstack/react-query";
import { impersonateUser } from "#/api/endpoints";
import { storeImpersonatedToken } from "#/server/auth";

type MutationOptions = {
	onError?: () => void;
};

/**
 * Signs the administrator in as somebody else and reloads the application as that person.
 *
 * Nothing here touches the auth store or the query cache, and that is the decision rather than an
 * omission: several cached keys do not carry a user id - "my profile", read by the top bar, and
 * every "mine" query in the time sheets - so clearing selectively would mean enumerating all of
 * them today and every one added later. A full navigation throws the whole cache away, re-runs the
 * route guard against the new cookie and starts from what the API says. The axios layer already
 * treats "this identity is no longer the one in play" as a hard reload for the same reason.
 *
 * There is no way back from here: the administrator's own session has been replaced, and returning
 * to it means signing out and signing in again.
 */
export function useImpersonateUser({ onError }: MutationOptions = {}) {
	return useMutation({
		mutationFn: (userId: string) => impersonateUser(userId),

		onSuccess: async (result) => {
			await storeImpersonatedToken({ data: { token: result.token } });

			window.location.href = "/app/dashboard";
		},

		onError,
	});
}
