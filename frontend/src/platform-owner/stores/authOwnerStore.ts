import { create } from "zustand";
import type { OwnerAuthenticated } from "@/api/models";

const TOKEN_KEY = "access_token";

interface OwnerAuthState {
	owner: OwnerAuthenticated | null;
	token: string | null;
	isAuthenticated: boolean;
	isLoading: boolean;
	hasCheckedAuth: boolean;
	setOwner: (user: OwnerAuthenticated | null) => void;
	setToken: (token: string | null) => void;
	setLoading: (loading: boolean) => void;
	getToken: () => string | null;
	clear: () => void;
}

export const useOwnerAuthStore = create<OwnerAuthState>((set, get) => ({
	owner: null,
	token: typeof window !== "undefined" ? localStorage.getItem(TOKEN_KEY) : null,
	isAuthenticated: false,
	isLoading: false,
	hasCheckedAuth: false,
	setOwner: (owner) =>
		set({
			owner,
			isAuthenticated: !!owner,
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
	clear: () => {
		localStorage.removeItem(TOKEN_KEY);
		set({
			owner: null,
			token: null,
			isAuthenticated: false,
			isLoading: false,
			hasCheckedAuth: true,
		});
	},
	setLoading: (loading) => set({ isLoading: loading }),
	getToken: () => get().token || localStorage.getItem(TOKEN_KEY),
}));
