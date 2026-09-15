import { useMutation, useQueryClient } from "@tanstack/react-query";
import { addMultipleTagToApplication, addMultipleTagToCandidate } from "#/api/endpoints";
import type { TagRequestList } from "#/api/models";
import { applicationKeys, candidatesKeys } from "#/api/query-keys";
import { useProjectionWait } from "#/hooks";

type TagTarget = "application" | "candidate";

type UseTagOptions = {
	onSuccess: () => void;
};

export function useTag({ onSuccess }: UseTagOptions) {
	const client = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const tagApplication = useMutation({
		mutationFn: ({ id, request }: { id: string; request: TagRequestList }) =>
			addMultipleTagToApplication(id, request),

		onSuccess: async (data) => {
			await wait();

			await client.invalidateQueries({
				queryKey: applicationKeys.detail(data.jobApplicationId),
			});

			await client.invalidateQueries({
				queryKey: applicationKeys.list(),
			});

			onSuccess();
		},
	});

	const tagCandidateMutation = useMutation({
		mutationFn: ({ id, request }: { id: string; request: TagRequestList }) =>
			addMultipleTagToCandidate(id, request),

		onSuccess: async (data) => {
			await wait();

			await client.invalidateQueries({
				queryKey: candidatesKeys.detail(data.candidateId),
			});
			await client.invalidateQueries({
				queryKey: candidatesKeys.list(),
			});

			onSuccess();
		},
	});

	const tag = (target: TagTarget, id: string, request: TagRequestList) => {
		if (target === "application") {
			tagApplication.reset();
			tagApplication.mutate({ id, request });
			return;
		}

		tagCandidateMutation.reset();
		tagCandidateMutation.mutate({ id, request });
	};

	return {
		tag,
		isPending: tagApplication.isPending || tagCandidateMutation.isPending || waiting,
		isError: tagApplication.isError || tagCandidateMutation.isError,
		error: tagApplication.error ?? tagCandidateMutation.error,
		waiting,
	};
}
