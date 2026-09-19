import { useMutation } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import {
	changeOpportunityStage,
	createFollowUpAction,
	createOpportunity,
	logSalesActivity,
	updateFollowUpAction,
	updateOpportunity,
} from "@/api/endpoints";
import type {
	ChangeOpportunityStageRequest,
	CreateFollowUpActionRequest,
	CreateOpportunityRequest,
	CreateSalesActivityRequest,
	OpportunityStage,
	UpdateFollowUpActionRequest,
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

type CreateFollowUpVariables = {
	request: CreateFollowUpActionRequest;
};

type UpdateFollowUpVariables = {
	followUpActionId: string;
	request: UpdateFollowUpActionRequest;
};

type OpportunityOptions = {
	onSuccess: () => void;
};

const createOpportuinityServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { req: CreateOpportunityRequest }) => input)
	.handler(async ({ data }) => {
		return createOpportunity(data.req, await getFnOptions());
	});

const updateOpportuinityServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; req: UpdateOpportunityRequest }) => input)
	.handler(async ({ data }) => {
		return updateOpportunity(data.id, data.req, await getFnOptions());
	});

const logActivityServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { req: CreateSalesActivityRequest }) => input)
	.handler(async ({ data }) => {
		return logSalesActivity(data.req, await getFnOptions());
	});

const createFollowUpServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { req: CreateFollowUpActionRequest }) => input)
	.handler(async ({ data }) => {
		return createFollowUpAction(data.req, await getFnOptions());
	});

const updateFollowUpServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; req: UpdateFollowUpActionRequest }) => input)
	.handler(async ({ data }) => {
		return updateFollowUpAction(data.id, data.req, await getFnOptions());
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
		mutation,
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
		mutation,
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
		mutation,
		waiting,
	};
}

export function useChangeStage({ onSuccess }: OpportunityOptions) {
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({
			oppotunityId,
			request,
		}: {
			oppotunityId: string;
			request: ChangeOpportunityStageRequest;
		}) => changeOpportunityStage(oppotunityId, request),

		onSuccess: async () => {
			await wait();
			onSuccess();
		},
	});

	const changeStage = (oppotunityId: string, stage: OpportunityStage, lostReason?: string) => {
		mutation.mutate({ oppotunityId, request: { stage: stage, lostReason } });
	};

	return {
		mutation,
		waiting,
		changeStage,
	};
}

export function useCreateFollowUp({ onSuccess }: OpportunityOptions) {
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ request }: CreateFollowUpVariables) =>
			createFollowUpServerFn({ data: { req: request } }),

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

export function useUpdateFollowUp({ onSuccess }: OpportunityOptions) {
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ followUpActionId, request }: UpdateFollowUpVariables) =>
			updateFollowUpServerFn({ data: { id: followUpActionId, req: request } }),

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
