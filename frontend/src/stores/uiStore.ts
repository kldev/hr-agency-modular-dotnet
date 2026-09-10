import { create } from "zustand";

const MODE = "mode";

export type Theme = "light" | "dark";

interface UiState {
	mode: Theme;
	getMode: () => Theme;
	setMode: (mode: Theme) => void;
}

const getMode = (): Theme => {
	const mode = typeof window !== "undefined" ? localStorage.getItem(MODE) : "";
	if (mode as Theme) {
		return mode as Theme;
	}
	return "dark";
};

export const useUiStore = create<UiState>((set, get) => ({
	mode: getMode(),
	getMode: () => get().mode,
	setMode: (mode: Theme) => {
		localStorage.setItem(MODE, mode);
		set({ mode: mode });
	},
}));
