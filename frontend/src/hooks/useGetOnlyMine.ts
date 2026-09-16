import { useAuthStore } from "#/stores/authStore";

type GetOnlyMineResult = {
	userId?: string;
};

export function useGetOnlyMine(value: boolean): GetOnlyMineResult {
	const { user } = useAuthStore();

	return {
		userId: value ? user?.userId : undefined,
	};
}
