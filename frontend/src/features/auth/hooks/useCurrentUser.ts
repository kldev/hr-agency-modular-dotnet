import { useNavigate } from "@tanstack/react-router";
import { useCallback } from "react";
import { getUserAuth } from "#/server/auth";
import { useAuthStore } from "@/stores/authStore";

export function useCurrentUser() {
	const nav = useNavigate();
	const { user, setUser, clearUser, setLoading } = useAuthStore();

	const checkAuth = useCallback(async () => {
		setLoading(true);
		try {
			const result = await getUserAuth();
			if (result === null) {
				clearUser();
				nav({ to: "/login" });
				return;
			}
			setUser(result);
		} catch {
			clearUser();
			nav({ to: "/login" });
		}
	}, [clearUser, setLoading, nav, setUser]);

	return {
		user,
		checkAuth,
	};
}
