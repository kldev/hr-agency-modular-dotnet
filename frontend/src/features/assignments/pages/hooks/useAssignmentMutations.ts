import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import {
	changeAssignmentStatus,
	planAssignment,
	recordAssignmentComplianceItem,
	removeAssignmentDocument,
	updateAssignment,
} from "@/api/endpoints";
import type {
	ChangeAssignmentStatusRequest,
	ComplianceRequirement,
	PlanAssignmentRequest,
	RecordAssignmentComplianceItemRequest,
	UpdateAssignmentRequest,
} from "@/api/models";
import { assignmentsKeys, workersKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

type MutationOptions = {
	/** Optional: most callers have nothing to add once the screen has already changed. */
	onSuccess?: () => void;
};

const planAssignmentServerFn = createServerFn({ method: "POST" })
	.validator((input: { req: PlanAssignmentRequest }) => input)
	.handler(async ({ data }) => {
		return planAssignment(data.req, await getFnOptions());
	});

const updateAssignmentServerFn = createServerFn({ method: "POST" })
	.validator((input: { id: string; req: UpdateAssignmentRequest }) => input)
	.handler(async ({ data }) => {
		return updateAssignment(data.id, data.req, await getFnOptions());
	});

const changeAssignmentStatusServerFn = createServerFn({ method: "POST" })
	.validator((input: { id: string; req: ChangeAssignmentStatusRequest }) => input)
	.handler(async ({ data }) => {
		return changeAssignmentStatus(data.id, data.req, await getFnOptions());
	});

const recordComplianceItemServerFn = createServerFn({ method: "POST" })
	.validator(
		(input: {
			id: string;
			requirement: ComplianceRequirement;
			req: RecordAssignmentComplianceItemRequest;
		}) => input,
	)
	.handler(async ({ data }) => {
		return recordAssignmentComplianceItem(
			data.id,
			data.requirement,
			data.req,
			await getFnOptions(),
		);
	});

const removeAssignmentDocumentServerFn = createServerFn({ method: "POST" })
	.validator((input: { id: string; documentId: string }) => input)
	.handler(async ({ data }) => {
		return removeAssignmentDocument(data.id, data.documentId, await getFnOptions());
	});

/*
 * The twin of `useWorkerMutation`, and it invalidates the same two keys for the same reason: a
 * posting starting or ending moves `currentWorkCountry` and the posting counts on the person's row,
 * because `WorkerProjection` is a MultiStreamProjection fed by both streams.
 */
export function useAssignmentMutation<TVariables, TResult>(
	mutationFn: (variables: TVariables) => Promise<TResult>,
	{ onSuccess }: MutationOptions,
) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn,

		onSuccess: async () => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: assignmentsKeys.all });
			await queryClient.invalidateQueries({ queryKey: workersKeys.all });

			onSuccess?.();
		},
	});

	return { mutation, waiting };
}

export function usePlanAssignment(options: MutationOptions) {
	return useAssignmentMutation(
		({ request }: { request: PlanAssignmentRequest }) =>
			planAssignmentServerFn({ data: { req: request } }),
		options,
	);
}

export function useUpdateAssignment(options: MutationOptions) {
	return useAssignmentMutation(
		({ assignmentId, request }: { assignmentId: string; request: UpdateAssignmentRequest }) =>
			updateAssignmentServerFn({ data: { id: assignmentId, req: request } }),
		options,
	);
}

export function useChangeAssignmentStatus(options: MutationOptions) {
	return useAssignmentMutation(
		({ assignmentId, request }: { assignmentId: string; request: ChangeAssignmentStatusRequest }) =>
			changeAssignmentStatusServerFn({ data: { id: assignmentId, req: request } }),
		options,
	);
}

export function useRecordAssignmentComplianceItem(options: MutationOptions) {
	return useAssignmentMutation(
		({
			assignmentId,
			requirement,
			request,
		}: {
			assignmentId: string;
			requirement: ComplianceRequirement;
			request: RecordAssignmentComplianceItemRequest;
		}) => recordComplianceItemServerFn({ data: { id: assignmentId, requirement, req: request } }),
		options,
	);
}

export function useRemoveAssignmentDocument(options: MutationOptions) {
	return useAssignmentMutation(
		({ assignmentId, documentId }: { assignmentId: string; documentId: string }) =>
			removeAssignmentDocumentServerFn({ data: { id: assignmentId, documentId } }),
		options,
	);
}
