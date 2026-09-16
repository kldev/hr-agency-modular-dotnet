export type OnSucess = {
	onSuccess: () => void;
};

export type PersonInfo = {
	fullName?: string;
	email?: string;
};
export type CardListProps<T> = {
	items: T[];
	onRefresh: () => void;
};
