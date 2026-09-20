import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import { completeCompanyProfile } from "@/api/endpoints";
import type { CompleteCompanyProfileRequest } from "@/api/models";
import { companiesKeys, projectsKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

const completeCompanyProfileServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; req: CompleteCompanyProfileRequest }) => input)
	.handler(async ({ data }) => {
		return completeCompanyProfile(data.id, data.req, await getFnOptions());
	});

/*
 * Both keys, deliberately. Whether a client's profile is complete is shown on the company and on
 * every project that points at it - a project cannot go live without it - so refreshing only the
 * company would leave the project still saying the data is missing.
 */
export function useCompleteCompanyProfile({ onSuccess }: { onSuccess: () => void }) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({
			companyId,
			request,
		}: {
			companyId: string;
			request: CompleteCompanyProfileRequest;
		}) => completeCompanyProfileServerFn({ data: { id: companyId, req: request } }),

		onSuccess: async () => {
			await wait();

			await Promise.all([
				queryClient.invalidateQueries({ queryKey: companiesKeys.all }),
				queryClient.invalidateQueries({ queryKey: projectsKeys.all }),
			]);

			onSuccess();
		},
	});

	return { mutation, waiting };
}
