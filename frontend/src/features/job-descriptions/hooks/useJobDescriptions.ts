import { useMutation } from "@tanstack/react-query";
import { createJobDescription } from "#/api/endpoints";
import type { CreateJobDescriptionRequest } from "#/api/models";
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
