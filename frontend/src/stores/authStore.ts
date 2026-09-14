import { create } from "zustand";
import type { AppUserAuthenticated } from "@/api/models";

interface AuthState {
	user: AppUserAuthenticated | null;

	isAuthenticated: boolean;
	isLoading: boolean;
	hasCheckedAuth: boolean;
	setUser: (user: AppUserAuthenticated | null) => void;
	setLoading: (loading: boolean) => void;
	clearUser: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
	user: null,

	isAuthenticated: false,
	isLoading: false,
	hasCheckedAuth: false,
	setUser: (user) =>
		set({
			user,
			isAuthenticated: !!user,
			isLoading: false,
			hasCheckedAuth: true,
		}),

	clearUser: () => {
		set({
			user: null,
			isAuthenticated: false,
			isLoading: false,
			hasCheckedAuth: true,
		});
	},
	setLoading: (loading) => set({ isLoading: loading }),
}));
