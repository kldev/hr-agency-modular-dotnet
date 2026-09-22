import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createJobPost, updateJobPost } from "#/api/endpoints";
import type { CreatePostRequest, UpdateJobPostRequest } from "#/api/models";
import { jobPostsKeys } from "#/api/query-keys";
import { useProjectionWait } from "#/hooks";

type CreateJobPostVariables = {
	request: CreatePostRequest;
};

/**
 * What to do once the write went through. Optional: a wizard closes its own dialog, so most callers
 * have nothing left to say.
 */
type MutationOptions = {
	onSuccess?: () => void;
};

export function useCreateJobPost({ onSuccess }: MutationOptions = {}) {
	const queryClient = useQueryClient();

	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ request }: CreateJobPostVariables) => createJobPost(request),

		onSuccess: async () => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: jobPostsKeys.all });

			onSuccess?.();
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

export function useUpdateJobPost({ onSuccess }: MutationOptions = {}) {
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

			onSuccess?.();
		},
	});

	return {
		mutation,
		waiting,
	};
}
