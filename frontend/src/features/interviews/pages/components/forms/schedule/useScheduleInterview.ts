import { useMutation, useQueryClient } from "@tanstack/react-query";
import { scheduleInterview } from "#/api/endpoints";
import type { ScheduleInterviewRequest } from "#/api/models";
import { interviewsKeys } from "#/api/query-keys";
import { useProjectionWait } from "#/hooks";

type UseScheduleInterviewOptions = {
	onSuccess: () => void;
};

export function useScheduleInterview({ onSuccess }: UseScheduleInterviewOptions) {
	const client = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({
			jobApplicationId,
			request,
		}: {
			jobApplicationId: string;
			request: ScheduleInterviewRequest;
		}) => scheduleInterview({ ...request, jobApplicationId }),

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
		schedule: (jobApplicationId: string, request: ScheduleInterviewRequest) => {
			mutation.reset();
			mutation.mutate({ jobApplicationId, request });
		},

		isPending: mutation.isPending || waiting,
		isError: mutation.isError,
		error: mutation.error,
		waiting,
		reset: mutation.reset,
	};
}
