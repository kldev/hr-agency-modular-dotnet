import { useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { getApiUserMe } from "@/api/endpoints";
import { ROUTES } from "@/routes";
import { useAuthStore } from "@/stores/authStore";

export function useCurrentUser() {
	const nav = useNavigate();
	const { user, setUser, clearUser, setLoading, getToken } = useAuthStore();

	const hasToken = !!getToken();

	const checkAuth = useCallback(async () => {
		if (!hasToken) {
			clearUser();
			nav(ROUTES.LOGIN);
			return;
		}

		setLoading(true);
		try {
			const result = await getApiUserMe();
			setUser(result);
		} catch {
			clearUser();
			nav(ROUTES.LOGIN);
		}
	}, [hasToken, clearUser, setLoading]);

	return {
		user,
		checkAuth,
		getToken,
	};
}
