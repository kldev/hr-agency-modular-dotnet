import { create } from "zustand";
import type { AppUserAuthenticated } from "@/api/models";

const TOKEN_KEY = "access_token";

interface AuthState {
	user: AppUserAuthenticated | null;
	token: string | null;
	isAuthenticated: boolean;
	isLoading: boolean;
	hasCheckedAuth: boolean;
	setUser: (user: AppUserAuthenticated | null) => void;
	setToken: (token: string | null) => void;
	setLoading: (loading: boolean) => void;
	getToken: () => string | null;
	clearUser: () => void;
}

export const useAuthStore = create<AuthState>((set, get) => ({
	user: null,
	token: typeof window !== "undefined" ? localStorage.getItem(TOKEN_KEY) : null,
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
	setToken: (token) => {
		if (token) {
			localStorage.setItem(TOKEN_KEY, token);
		} else {
			localStorage.removeItem(TOKEN_KEY);
		}
		set({ token });
	},
	clearUser: () => {
		localStorage.removeItem(TOKEN_KEY);
		set({
			user: null,
			token: null,
			isAuthenticated: false,
			isLoading: false,
			hasCheckedAuth: true,
		});
	},
	setLoading: (loading) => set({ isLoading: loading }),
	getToken: () => get().token || localStorage.getItem(TOKEN_KEY),
}));
