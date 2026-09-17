import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createJobPost, updateJobPost } from "#/api/endpoints";
import type { CreatePostRequest, UpdateJobPostRequest } from "#/api/models";
import { jobPostsKeys } from "#/api/query-keys";
import { useProjectionWait } from "#/hooks";
import type { OnSucess } from "#/types";

type CreateJobPostVariables = {
	request: CreatePostRequest;
};

export function useCreateJobPost({ onSuccess }: OnSucess) {
	const queryClient = useQueryClient();

	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ request }: CreateJobPostVariables) => createJobPost(request),

		onSuccess: async () => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: jobPostsKeys.all });

			onSuccess();
		},
	});

	return {
		mutation,
		waiting,
	};
}

type UpdateJobPostVariables = {
	jobPostId: string;
	request: UpdateJobPostRequest;
};

export function useUpdateJobPost({ onSuccess }: OnSucess) {
	const queryClient = useQueryClient();

	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ jobPostId, request }: UpdateJobPostVariables) =>
			updateJobPost(jobPostId, request),

		onSuccess: async () => {
			await wait();

			/*
			 * Details and list at once - both read projections rebuilt from the same event, and the
			 * user lands on the details page right after saving.
			 */
			await queryClient.invalidateQueries({ queryKey: jobPostsKeys.all });

			onSuccess();
		},
	});

	return {
		mutation,
		waiting,
	};
}
