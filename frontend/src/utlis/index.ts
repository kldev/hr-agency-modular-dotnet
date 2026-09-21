export function removeFromArray<T>(items: T[], predicate: (item: T) => boolean): T[] {
	return items.filter((item) => !predicate(item));
}

export function addIfNotExists<T>(
	items: T[],
	item: T,
	isSame: (a: T, b: T) => boolean,
): { items: T[]; added: boolean } {
	if (items.some((existing) => isSame(existing, item))) {
		return {
			items,
			added: false,
		};
	}

	return {
		items: [...items, item],
		added: true,
	};
}
export * from "./copyToClipboard";
export * from "./dateUtils";
export * from "./delay";
export * from "./documentUpload";
export * from "./formatFileSize";
export * from "./formatRecord";
export * from "./formatSalary";
export * from "./generatePassword";
