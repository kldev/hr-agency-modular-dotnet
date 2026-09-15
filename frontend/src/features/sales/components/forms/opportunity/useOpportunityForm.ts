import { useMutation } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import { createOpportunity, logSalesActivity, updateOpportunity } from "@/api/endpoints";
import type {
	CreateOpportunityRequest,
	CreateSalesActivityRequest,
	UpdateOpportunityRequest,
} from "@/api/models";
import { useProjectionWait } from "@/hooks";

type CreateOpportunityVariables = {
	request: CreateOpportunityRequest;
};

type UpdateOpportunityVariables = {
	id: string;
	request: UpdateOpportunityRequest;
};

type LogAcivityVariables = {
	request: CreateSalesActivityRequest;
};

type OpportunityOptions = {
	onSuccess: () => void;
};

const createOpportuinityServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { req: CreateOpportunityRequest }) => input)
	.handler(({ data }) => {
		return createOpportunity(data.req, getFnOptions());
	});

const updateOpportuinityServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; req: UpdateOpportunityRequest }) => input)
	.handler(({ data }) => {
		return updateOpportunity(data.id, data.req, getFnOptions());
	});

const logActivityServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { req: CreateSalesActivityRequest }) => input)
	.handler(({ data }) => {
		return logSalesActivity(data.req, getFnOptions());
	});

export function useCreateOpportuinity({ onSuccess }: OpportunityOptions) {
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ request }: CreateOpportunityVariables) =>
			createOpportuinityServerFn({ data: { req: request } }),

		onSuccess: async () => {
			await wait();
			onSuccess();
		},
	});

	return {
		isPending: mutation.isPending,
		isError: mutation.isError,
		error: mutation.error,
		waiting,
	};
}

export function useUpdateOpportuinity({ onSuccess }: OpportunityOptions) {
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ id, request }: UpdateOpportunityVariables) =>
			updateOpportuinityServerFn({ data: { id: id, req: request } }),

		onSuccess: async () => {
			await wait();
			onSuccess();
		},
	});

	return {
		isPending: mutation.isPending,
		isError: mutation.isError,
		error: mutation.error,
		waiting,
	};
}

export function useLogActivity({ onSuccess }: OpportunityOptions) {
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ request }: LogAcivityVariables) =>
			logActivityServerFn({ data: { req: request } }),

		onSuccess: async () => {
			await wait();
			onSuccess();
		},
	});

	return {
		isPending: mutation.isPending,
		isError: mutation.isError,
		error: mutation.error,
		waiting,
	};
}
