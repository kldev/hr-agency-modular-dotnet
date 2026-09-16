import { useMutation, useQueryClient } from "@tanstack/react-query";
import { rescheduleInterview } from "#/api/endpoints";
import type { RescheduleInterviewRequest } from "#/api/models";
import { interviewsKeys } from "#/api/query-keys";
import { useProjectionWait } from "#/hooks";
import type { OnSucess } from "#/types";

export function useRescheduleInterview({ onSuccess }: OnSucess) {
	const client = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({
			interviewId,
			request,
		}: {
			interviewId: string;
			request: RescheduleInterviewRequest;
		}) => rescheduleInterview(interviewId, request),

		onSuccess: async (data) => {
			await wait();

			await client.invalidateQueries({
				queryKey: interviewsKeys.lists(),
			});

			await client.invalidateQueries({
				queryKey: interviewsKeys.details(data.interviewId),
			});

			onSuccess();
		},
	});

	return {
		schedule: (interviewId: string, request: RescheduleInterviewRequest) => {
			mutation.reset();
			mutation.mutate({ interviewId, request });
		},

		isPending: mutation.isPending || waiting,
		isError: mutation.isError,
		error: mutation.error,
		waiting,
		reset: mutation.reset,
	};
}
