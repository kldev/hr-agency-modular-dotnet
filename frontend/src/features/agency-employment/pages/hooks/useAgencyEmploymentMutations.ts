import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import {
	changeAgencyEmploymentTerms,
	endAgencyEmployment,
	startAgencyEmployment,
} from "@/api/endpoints";
import type {
	AgencyEmploymentEnded,
	AgencyEmploymentStarted,
	AgencyEmploymentTermsChanged,
	ChangeAgencyEmploymentTermsRequest,
	EndAgencyEmploymentRequest,
	StartAgencyEmploymentRequest,
} from "@/api/models";
import { agencyEmploymentKeys, timeSheetsKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

type MutationOptions<TResult = unknown> = {
	onSuccess: (result: TResult) => void;
	onError?: (error: unknown) => void;
};

const startServerFn = createServerFn({ method: "POST" })
	.validator((input: { req: StartAgencyEmploymentRequest }) => input)
	.handler(async ({ data }) => startAgencyEmployment(data.req, await getFnOptions()));

const changeTermsServerFn = createServerFn({ method: "POST" })
	.validator((input: { userId: string; req: ChangeAgencyEmploymentTermsRequest }) => input)
	.handler(async ({ data }) =>
		changeAgencyEmploymentTerms(data.userId, data.req, await getFnOptions()),
	);

const endServerFn = createServerFn({ method: "POST" })
	.validator((input: { userId: string; req: EndAgencyEmploymentRequest }) => input)
	.handler(async ({ data }) => endAgencyEmployment(data.userId, data.req, await getFnOptions()));

/*
 * Both keys, every time. The employment register decides who is covered by the duty to record
 * hours, and the time sheet monitoring is built by joining that answer with the sheets - so
 * putting somebody on a mandate adds a row to a screen that lives under a different key entirely.
 */
function useAgencyEmploymentMutation<TVariables, TResult>(
	mutationFn: (variables: TVariables) => Promise<TResult>,
	{ onSuccess, onError }: MutationOptions<TResult>,
) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn,

		onSuccess: async (result) => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: agencyEmploymentKeys.all });
			await queryClient.invalidateQueries({ queryKey: timeSheetsKeys.all });

			onSuccess(result);
		},

		onError,
	});

	return { mutation, waiting };
}

export function useStartAgencyEmployment(options: MutationOptions<AgencyEmploymentStarted>) {
	return useAgencyEmploymentMutation(
		({ request }: { request: StartAgencyEmploymentRequest }) =>
			startServerFn({ data: { req: request } }),
		options,
	);
}

export function useChangeAgencyEmploymentTerms(
	options: MutationOptions<AgencyEmploymentTermsChanged>,
) {
	return useAgencyEmploymentMutation(
		({ userId, request }: { userId: string; request: ChangeAgencyEmploymentTermsRequest }) =>
			changeTermsServerFn({ data: { userId, req: request } }),
		options,
	);
}

export function useEndAgencyEmployment(options: MutationOptions<AgencyEmploymentEnded>) {
	return useAgencyEmploymentMutation(
		({ userId, request }: { userId: string; request: EndAgencyEmploymentRequest }) =>
			endServerFn({ data: { userId, req: request } }),
		options,
	);
}
