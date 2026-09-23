import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import {
	addStandardSystemFields,
	archiveForm,
	archiveSystemField,
	correctFormResponse,
	createForm,
	defineSystemField,
	previewFormLayout,
	publishForm,
	saveFormDraft,
	saveFormResponseDraft,
	startFormResponse,
	submitFormResponse,
	updateFormDetails,
	updateSystemField,
} from "@/api/endpoints";
import type {
	AnswersRequest,
	CorrectFormResponseRequest,
	CreateFormRequest,
	DefineSystemFieldRequest,
	PreviewLayoutRequest,
	SaveFormDraftRequest,
	StartFormResponseRequest,
	UpdateFormDetailsRequest,
	UpdateSystemFieldRequest,
} from "@/api/models";
import { formResponsesKeys, formsKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

type MutationOptions<TResult> = {
	onSuccess?: (result: TResult) => void;
};

const createFormServerFn = createServerFn({ method: "POST" })
	.validator((input: CreateFormRequest) => input)
	.handler(async ({ data }) => createForm(data, await getFnOptions()));

const updateFormDetailsServerFn = createServerFn({ method: "POST" })
	.validator((input: { formId: string; req: UpdateFormDetailsRequest }) => input)
	.handler(async ({ data }) => updateFormDetails(data.formId, data.req, await getFnOptions()));

const saveFormDraftServerFn = createServerFn({ method: "POST" })
	.validator((input: { formId: string; req: SaveFormDraftRequest }) => input)
	.handler(async ({ data }) => saveFormDraft(data.formId, data.req, await getFnOptions()));

const previewFormLayoutServerFn = createServerFn({ method: "POST" })
	.validator((input: { formId: string; req: PreviewLayoutRequest }) => input)
	.handler(async ({ data }) => previewFormLayout(data.formId, data.req, await getFnOptions()));

const publishFormServerFn = createServerFn({ method: "POST" })
	.validator((input: string) => input)
	.handler(async ({ data }) => publishForm(data, await getFnOptions()));

const archiveFormServerFn = createServerFn({ method: "POST" })
	.validator((input: string) => input)
	.handler(async ({ data }) => archiveForm(data, await getFnOptions()));

const defineSystemFieldServerFn = createServerFn({ method: "POST" })
	.validator((input: DefineSystemFieldRequest) => input)
	.handler(async ({ data }) => defineSystemField(data, await getFnOptions()));

const updateSystemFieldServerFn = createServerFn({ method: "POST" })
	.validator((input: { id: string; req: UpdateSystemFieldRequest }) => input)
	.handler(async ({ data }) => updateSystemField(data.id, data.req, await getFnOptions()));

const archiveSystemFieldServerFn = createServerFn({ method: "POST" })
	.validator((input: string) => input)
	.handler(async ({ data }) => archiveSystemField(data, await getFnOptions()));

const addStandardSystemFieldsServerFn = createServerFn({ method: "POST" }).handler(async () =>
	addStandardSystemFields(await getFnOptions()),
);

const startFormResponseServerFn = createServerFn({ method: "POST" })
	.validator((input: StartFormResponseRequest) => input)
	.handler(async ({ data }) => startFormResponse(data, await getFnOptions()));

const saveFormResponseDraftServerFn = createServerFn({ method: "POST" })
	.validator((input: { responseId: string; req: AnswersRequest }) => input)
	.handler(async ({ data }) =>
		saveFormResponseDraft(data.responseId, data.req, await getFnOptions()),
	);

const submitFormResponseServerFn = createServerFn({ method: "POST" })
	.validator((input: { responseId: string; req: AnswersRequest }) => input)
	.handler(async ({ data }) => submitFormResponse(data.responseId, data.req, await getFnOptions()));

const correctFormResponseServerFn = createServerFn({ method: "POST" })
	.validator((input: { responseId: string; req: CorrectFormResponseRequest }) => input)
	.handler(async ({ data }) =>
		correctFormResponse(data.responseId, data.req, await getFnOptions()),
	);

/*
 * Definitions: the builder and the catalogue read replayed aggregates, so they are fresh the moment a
 * command returns; only the list is a projection. The wait is for the list, and both keys go.
 */
function useFormsMutation<TVariables, TResult>(
	mutationFn: (variables: TVariables) => Promise<TResult>,
	{ onSuccess }: MutationOptions<TResult>,
	waitForList = true,
) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn,
		onSuccess: async (result) => {
			if (waitForList) {
				await wait();
			}

			await queryClient.invalidateQueries({ queryKey: formsKeys.all });
			await queryClient.invalidateQueries({ queryKey: formResponsesKeys.all });
			onSuccess?.(result);
		},
	});

	return { mutation, waiting };
}

export const useCreateForm = (options: MutationOptions<unknown> = {}) =>
	useFormsMutation((req: CreateFormRequest) => createFormServerFn({ data: req }), options);

export const useUpdateFormDetails = (options: MutationOptions<unknown> = {}) =>
	useFormsMutation(
		(input: { formId: string; req: UpdateFormDetailsRequest }) =>
			updateFormDetailsServerFn({ data: input }),
		options,
	);

/** Saved often while building, so it does not wait on the list: the builder reads the aggregate. */
export const useSaveFormDraft = (options: MutationOptions<unknown> = {}) =>
	useFormsMutation(
		(input: { formId: string; req: SaveFormDraftRequest }) =>
			saveFormDraftServerFn({ data: input }),
		options,
		false,
	);

export function usePreviewFormLayout() {
	return useMutation({
		mutationFn: (input: { formId: string; req: PreviewLayoutRequest }) =>
			previewFormLayoutServerFn({ data: input }),
	});
}

export const usePublishForm = (options: MutationOptions<unknown> = {}) =>
	useFormsMutation((formId: string) => publishFormServerFn({ data: formId }), options);

export const useArchiveForm = (options: MutationOptions<unknown> = {}) =>
	useFormsMutation((formId: string) => archiveFormServerFn({ data: formId }), options);

export const useDefineSystemField = (options: MutationOptions<unknown> = {}) =>
	useFormsMutation(
		(req: DefineSystemFieldRequest) => defineSystemFieldServerFn({ data: req }),
		options,
		false,
	);

export const useUpdateSystemField = (options: MutationOptions<unknown> = {}) =>
	useFormsMutation(
		(input: { id: string; req: UpdateSystemFieldRequest }) =>
			updateSystemFieldServerFn({ data: input }),
		options,
		false,
	);

export const useArchiveSystemField = (options: MutationOptions<unknown> = {}) =>
	useFormsMutation((id: string) => archiveSystemFieldServerFn({ data: id }), options, false);

export const useAddStandardSystemFields = (options: MutationOptions<unknown> = {}) =>
	useFormsMutation(() => addStandardSystemFieldsServerFn(), options, false);

/*
 * Responses: a single response is replayed, so it is fresh; the worker's tab is a projection, so a
 * start or a submission waits before the list is refetched.
 */
export const useStartFormResponse = (
	options: MutationOptions<Awaited<ReturnType<typeof startFormResponse>>> = {},
) =>
	useFormsMutation(
		(req: StartFormResponseRequest) => startFormResponseServerFn({ data: req }),
		options,
	);

export const useSaveFormResponseDraft = (options: MutationOptions<unknown> = {}) =>
	useFormsMutation(
		(input: { responseId: string; req: AnswersRequest }) =>
			saveFormResponseDraftServerFn({ data: input }),
		options,
		false,
	);

export const useSubmitFormResponse = (options: MutationOptions<unknown> = {}) =>
	useFormsMutation(
		(input: { responseId: string; req: AnswersRequest }) =>
			submitFormResponseServerFn({ data: input }),
		options,
	);

export const useCorrectFormResponse = (options: MutationOptions<unknown> = {}) =>
	useFormsMutation(
		(input: { responseId: string; req: CorrectFormResponseRequest }) =>
			correctFormResponseServerFn({ data: input }),
		options,
	);
