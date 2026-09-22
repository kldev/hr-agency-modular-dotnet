import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createJobDescription, updateJobDescription } from "#/api/endpoints";
import type { CreateJobDescriptionRequest, UpdateJobDescriptionRequest } from "#/api/models";
import { jobDescriptionKeys } from "#/api/query-keys";
import { useProjectionWait } from "#/hooks";

type CreateJobDescriptionVariables = {
	request: CreateJobDescriptionRequest;
};

/**
 * What to do once the write went through. Optional: a wizard closes its own dialog, so most callers
 * have nothing left to say.
 */
type MutationOptions = {
	onSuccess?: () => void;
};

export function useCreateJobDescription({ onSuccess }: MutationOptions = {}) {
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ request }: CreateJobDescriptionVariables) => createJobDescription(request),

		onSuccess: async () => {
			await wait();
			onSuccess?.();
		},
	});

	return {
		mutation,
		waiting,
	};
}

type UpdateJobDescriptionVariables = {
	jobDescriptionId: string;
	request: UpdateJobDescriptionRequest;
};

export function useUpdateJobDescription({ onSuccess }: MutationOptions = {}) {
	const queryClient = useQueryClient();

	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ jobDescriptionId, request }: UpdateJobDescriptionVariables) =>
			updateJobDescription(jobDescriptionId, request),

		onSuccess: async () => {
			await wait();

			/*
			 * Details and list at once - both read projections rebuilt from the same event, and the
			 * user lands on the details page right after saving.
			 */
			await queryClient.invalidateQueries({ queryKey: jobDescriptionKeys.all });

			onSuccess?.();
		},
	});

	return {
		mutation,
		waiting,
	};
}
