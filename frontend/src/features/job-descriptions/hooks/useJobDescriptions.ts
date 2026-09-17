import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createJobDescription, updateJobDescription } from "#/api/endpoints";
import type { CreateJobDescriptionRequest, UpdateJobDescriptionRequest } from "#/api/models";
import { jobDescriptionKeys } from "#/api/query-keys";
import { useProjectionWait } from "#/hooks";
import type { OnSucess } from "#/types";

type CreateJobDescriptionVariables = {
	request: CreateJobDescriptionRequest;
};

export function useCreateJobDescription({ onSuccess }: OnSucess) {
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ request }: CreateJobDescriptionVariables) => createJobDescription(request),

		onSuccess: async () => {
			await wait();
			onSuccess();
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

export function useUpdateJobDescription({ onSuccess }: OnSucess) {
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

			onSuccess();
		},
	});

	return {
		mutation,
		waiting,
	};
}
