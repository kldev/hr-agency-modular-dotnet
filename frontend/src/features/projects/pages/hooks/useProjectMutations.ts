import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import {
	assignProjectContact,
	assignProjectTeam,
	changeProjectLegalEntity,
	changeProjectStatus,
	createProject,
	recordComplianceItem,
	recordProjectContract,
	removeProjectContact,
	removeProjectDocument,
	setProjectEmailRecipients,
	updateProject,
} from "@/api/endpoints";
import type {
	AssignProjectContactRequest,
	AssignProjectTeamRequest,
	ChangeProjectLegalEntityRequest,
	ChangeProjectStatusRequest,
	ComplianceRequirement,
	ContactRole,
	CreateProjectRequest,
	EmailPurpose,
	RecordComplianceItemRequest,
	RecordProjectContractRequest,
	SetProjectEmailRecipientsRequest,
	UpdateProjectRequest,
} from "@/api/models";
import { projectsKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

type MutationOptions = {
	/** Optional: most callers have nothing to add once the screen has already changed. */
	onSuccess?: () => void;
};

const createProjectServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { req: CreateProjectRequest }) => input)
	.handler(async ({ data }) => {
		return createProject(data.req, await getFnOptions());
	});

const updateProjectServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; req: UpdateProjectRequest }) => input)
	.handler(async ({ data }) => {
		return updateProject(data.id, data.req, await getFnOptions());
	});

const changeProjectLegalEntityServerFn = createServerFn({ method: "POST" })
	.validator((input: { id: string; req: ChangeProjectLegalEntityRequest }) => input)
	.handler(async ({ data }) => {
		return changeProjectLegalEntity(data.id, data.req, await getFnOptions());
	});

const changeProjectStatusServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; req: ChangeProjectStatusRequest }) => input)
	.handler(async ({ data }) => {
		return changeProjectStatus(data.id, data.req, await getFnOptions());
	});

const assignProjectTeamServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; req: AssignProjectTeamRequest }) => input)
	.handler(async ({ data }) => {
		return assignProjectTeam(data.id, data.req, await getFnOptions());
	});

const assignProjectContactServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; role: ContactRole; req: AssignProjectContactRequest }) => input)
	.handler(async ({ data }) => {
		return assignProjectContact(data.id, data.role, data.req, await getFnOptions());
	});

const removeProjectContactServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; role: ContactRole }) => input)
	.handler(async ({ data }) => {
		return removeProjectContact(data.id, data.role, await getFnOptions());
	});

const setProjectEmailRecipientsServerFn = createServerFn({
	method: "POST",
})
	.validator(
		(input: { id: string; purpose: EmailPurpose; req: SetProjectEmailRecipientsRequest }) => input,
	)
	.handler(async ({ data }) => {
		return setProjectEmailRecipients(data.id, data.purpose, data.req, await getFnOptions());
	});

const recordProjectContractServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; req: RecordProjectContractRequest }) => input)
	.handler(async ({ data }) => {
		return recordProjectContract(data.id, data.req, await getFnOptions());
	});

const recordComplianceItemServerFn = createServerFn({
	method: "POST",
})
	.validator(
		(input: { id: string; requirement: ComplianceRequirement; req: RecordComplianceItemRequest }) =>
			input,
	)
	.handler(async ({ data }) => {
		return recordComplianceItem(data.id, data.requirement, data.req, await getFnOptions());
	});

const removeProjectDocumentServerFn = createServerFn({
	method: "POST",
})
	.validator((input: { id: string; documentId: string }) => input)
	.handler(async ({ data }) => {
		return removeProjectDocument(data.id, data.documentId, await getFnOptions());
	});

/*
 * Every project mutation invalidates the whole projects key, and waits first.
 *
 * The wait is not decoration: the details page reads a Marten projection that an async daemon
 * rebuilds, so a refetch fired the moment the command returns shows the state before the change.
 * The invalidation is broad because one command moves several screens at once - recording a
 * contract changes the details page, the compliance catalogue and the "outstanding" count the list
 * shows in its own column.
 */
function useProjectMutation<TVariables, TResult>(
	mutationFn: (variables: TVariables) => Promise<TResult>,
	{ onSuccess }: MutationOptions,
) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn,

		onSuccess: async () => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: projectsKeys.all });

			onSuccess?.();
		},
	});

	return { mutation, waiting };
}

export function useCreateProject(options: MutationOptions) {
	return useProjectMutation(
		({ request }: { request: CreateProjectRequest }) =>
			createProjectServerFn({ data: { req: request } }),
		options,
	);
}

export function useUpdateProject(options: MutationOptions) {
	return useProjectMutation(
		({ projectId, request }: { projectId: string; request: UpdateProjectRequest }) =>
			updateProjectServerFn({ data: { id: projectId, req: request } }),
		options,
	);
}

export function useChangeProjectStatus(options: MutationOptions) {
	return useProjectMutation(
		({ projectId, request }: { projectId: string; request: ChangeProjectStatusRequest }) =>
			changeProjectStatusServerFn({ data: { id: projectId, req: request } }),
		options,
	);
}

export function useChangeProjectLegalEntity(options: MutationOptions) {
	return useProjectMutation(
		({ projectId, legalEntityId }: { projectId: string; legalEntityId: string }) =>
			changeProjectLegalEntityServerFn({ data: { id: projectId, req: { legalEntityId } } }),
		options,
	);
}

export function useAssignProjectTeam(options: MutationOptions) {
	return useProjectMutation(
		({ projectId, request }: { projectId: string; request: AssignProjectTeamRequest }) =>
			assignProjectTeamServerFn({ data: { id: projectId, req: request } }),
		options,
	);
}

export function useAssignProjectContact(options: MutationOptions) {
	return useProjectMutation(
		({
			projectId,
			role,
			request,
		}: {
			projectId: string;
			role: ContactRole;
			request: AssignProjectContactRequest;
		}) => assignProjectContactServerFn({ data: { id: projectId, role, req: request } }),
		options,
	);
}

export function useRemoveProjectContact(options: MutationOptions) {
	return useProjectMutation(
		({ projectId, role }: { projectId: string; role: ContactRole }) =>
			removeProjectContactServerFn({ data: { id: projectId, role } }),
		options,
	);
}

export function useSetProjectEmailRecipients(options: MutationOptions) {
	return useProjectMutation(
		({
			projectId,
			purpose,
			request,
		}: {
			projectId: string;
			purpose: EmailPurpose;
			request: SetProjectEmailRecipientsRequest;
		}) => setProjectEmailRecipientsServerFn({ data: { id: projectId, purpose, req: request } }),
		options,
	);
}

export function useRecordProjectContract(options: MutationOptions) {
	return useProjectMutation(
		({ projectId, request }: { projectId: string; request: RecordProjectContractRequest }) =>
			recordProjectContractServerFn({ data: { id: projectId, req: request } }),
		options,
	);
}

export function useRecordComplianceItem(options: MutationOptions) {
	return useProjectMutation(
		({
			projectId,
			requirement,
			request,
		}: {
			projectId: string;
			requirement: ComplianceRequirement;
			request: RecordComplianceItemRequest;
		}) => recordComplianceItemServerFn({ data: { id: projectId, requirement, req: request } }),
		options,
	);
}

export function useRemoveProjectDocument(options: MutationOptions) {
	return useProjectMutation(
		({ projectId, documentId }: { projectId: string; documentId: string }) =>
			removeProjectDocumentServerFn({ data: { id: projectId, documentId } }),
		options,
	);
}
