import type React from "react";
import { useEffect, useRef } from "react";
import { useCurrentOwner } from "@/platform-owner/features/auth/hooks/useCurrentOwner";

interface AuthProviderProps {
	children: React.ReactNode;
}

const OwnerAuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
	const { checkAuth, getToken } = useCurrentOwner();
	const hasChecked = useRef(false);
	const hasToken = !!getToken();

	// Auto-fetch on mount if has token
	useEffect(() => {
		if (hasToken) {
			checkAuth();
		}
	}, [hasToken]);

	useEffect(() => {
		if (!hasChecked.current) {
			hasChecked.current = true;
			checkAuth();
		}
	}, [checkAuth]);

	return <>{children}</>;
};

export default OwnerAuthProvider;
