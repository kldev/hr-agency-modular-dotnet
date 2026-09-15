import { useMutation } from "@tanstack/react-query";
import { applyToJobPost } from "@/api/endpoints";
import type { ApplyToPostRequest } from "@/api/models";
import { useProjectionWait } from "@/hooks";

type ApplyToJobPostVariables = {
	id: string;
	request: ApplyToPostRequest;
};

type UseApplyToJobPostOptions = {
	onSuccess: () => void;
};

export function useApplyToJobPost({ onSuccess }: UseApplyToJobPostOptions) {
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ id, request }: ApplyToJobPostVariables) => applyToJobPost(id, request),

		onSuccess: async () => {
			await wait();
			onSuccess();
		},
	});

	return {
		...mutation,
		waiting,
	};
}
