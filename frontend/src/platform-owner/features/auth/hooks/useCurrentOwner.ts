import { useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { getApiOwnerMe } from "@/api/endpoints";
import { useOwnerAuthStore } from "@/platform-owner/stores/authOwnerStore";
import { OWNER_ROUTES } from "@/routes/OwnerRoutes";

export function useCurrentOwner() {
	const nav = useNavigate();
	const { owner, setOwner, clear, setLoading, getToken } = useOwnerAuthStore();

	const hasToken = !!getToken();

	const checkAuth = useCallback(async () => {
		if (!hasToken) {
			clear();
			nav(OWNER_ROUTES.LOGIN);
			return;
		}

		setLoading(true);
		try {
			const result = await getApiOwnerMe();
			setOwner(result);
		} catch {
			clear();
			nav(OWNER_ROUTES.LOGIN);
		}
	}, [hasToken, clear, setLoading]);

	return {
		owner,
		checkAuth,
		getToken,
	};
}
