import { useNavigate } from "@tanstack/react-router";
import { useCallback } from "react";
import { getOwnerAuth } from "#/server/auth";
import { useOwnerAuthStore } from "@/platform-owner/stores/authOwnerStore";

export function useCurrentOwner() {
	const nav = useNavigate();
	const { owner, setOwner, clear, setLoading } = useOwnerAuthStore();

	const checkAuth = useCallback(async () => {
		setLoading(true);
		try {
			const result = await getOwnerAuth();
			if (!result) {
				clear();
				nav({ to: "/admin" });
				return;
			}
			setOwner(result);
		} catch {
			clear();
			nav({ to: "/admin" });
		}
	}, [clear, setLoading, nav, setOwner]);

	return {
		owner,
		checkAuth,
	};
}
