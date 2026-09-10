import type React from "react";
import { useEffect, useRef } from "react";
import { useCurrentUser } from "@/features/auth/hooks/useCurrentUser";

interface AuthProviderProps {
	children: React.ReactNode;
}

const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
	const { checkAuth, getToken } = useCurrentUser();
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

	//return <div>Auth</div>
	return <>{children}</>;
};

export default AuthProvider;
