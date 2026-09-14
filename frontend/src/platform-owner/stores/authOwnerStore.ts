import { create } from "zustand";
import type { OwnerAuthenticated } from "@/api/models";

interface OwnerAuthState {
	owner: OwnerAuthenticated | null;
	token: string | null;
	isAuthenticated: boolean;
	isLoading: boolean;
	hasCheckedAuth: boolean;
	setOwner: (user: OwnerAuthenticated | null) => void;
	setLoading: (loading: boolean) => void;
	clear: () => void;
}

export const useOwnerAuthStore = create<OwnerAuthState>((set) => ({
	owner: null,
	token: null,
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

	clear: () => {
		set({
			owner: null,
			token: null,
			isAuthenticated: false,
			isLoading: false,
			hasCheckedAuth: true,
		});
	},
	setLoading: (loading) => set({ isLoading: loading }),
}));
