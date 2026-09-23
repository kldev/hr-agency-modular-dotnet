import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import {
	getForm,
	getFormResponse,
	getForms,
	getSubjectAvailableForms,
	getSubjectFormResponses,
	getSystemFields,
} from "@/api/endpoints";
import type { FormKind, FormStatus } from "@/api/models";
import { formResponsesKeys, formsKeys } from "@/api/query-keys";

const PAGE_SIZE = 20;

export type FormsFilters = {
	search?: string;
	status?: FormStatus[];
	kind?: FormKind[];
};

const getFormsServerFn = createServerFn({ method: "GET" })
	.validator((input: FormsFilters & { page: number; pageSize: number }) => input)
	.handler(async ({ data }) =>
		getForms(
			{
				search: data.search ?? "",
				...(data.status?.length ? { status: data.status } : {}),
				...(data.kind?.length ? { kind: data.kind } : {}),
				page: data.page,
				pageSize: data.pageSize,
			},
			await getFnOptions(),
		),
	);

const getFormServerFn = createServerFn({ method: "GET" })
	.validator((input: string) => input)
	.handler(async ({ data }) => getForm(data, await getFnOptions()));

const getSystemFieldsServerFn = createServerFn({ method: "GET" })
	.validator((input: boolean) => input)
	.handler(async ({ data }) => getSystemFields({ includeArchived: data }, await getFnOptions()));

const getWorkerResponsesServerFn = createServerFn({ method: "GET" })
	.validator((input: string) => input)
	.handler(async ({ data }) => getSubjectFormResponses("worker", data, await getFnOptions()));

const getWorkerAvailableFormsServerFn = createServerFn({ method: "GET" })
	.validator((input: string) => input)
	.handler(async ({ data }) => getSubjectAvailableForms("worker", data, await getFnOptions()));

const getFormResponseServerFn = createServerFn({ method: "GET" })
	.validator((input: string) => input)
	.handler(async ({ data }) => getFormResponse(data, await getFnOptions()));

export function useGetFormsSlice(filter: FormsFilters) {
	return useInfiniteQuery({
		queryKey: formsKeys.list(filter),
		initialPageParam: 1,
		queryFn: ({ pageParam }) =>
			getFormsServerFn({ data: { ...filter, page: pageParam, pageSize: PAGE_SIZE } }),
		getNextPageParam: (lastPage, _pages, lastPageParam) =>
			lastPage.hasMore ? lastPageParam + 1 : undefined,
	});
}

/** The builder's read: replayed on the backend, so it is the draft that was just saved. */
export function useGetForm(formId: string) {
	return useQuery({
		queryKey: formsKeys.details(formId),
		queryFn: () => getFormServerFn({ data: formId }),
		enabled: Boolean(formId),
	});
}

export function useGetSystemFields(includeArchived = false) {
	return useQuery({
		queryKey: formsKeys.systemFields(includeArchived),
		queryFn: () => getSystemFieldsServerFn({ data: includeArchived }),
	});
}

export function useGetWorkerFormResponses(workerId: string) {
	return useQuery({
		queryKey: formResponsesKeys.forSubject("worker", workerId),
		queryFn: () => getWorkerResponsesServerFn({ data: workerId }),
		enabled: Boolean(workerId),
	});
}

export function useGetWorkerAvailableForms(workerId: string, enabled: boolean) {
	return useQuery({
		queryKey: formResponsesKeys.available("worker", workerId),
		queryFn: () => getWorkerAvailableFormsServerFn({ data: workerId }),
		enabled: enabled && Boolean(workerId),
	});
}

/** One response with the frozen version it belongs to - everything the renderer needs. */
export function useGetFormResponse(responseId: string | null) {
	return useQuery({
		queryKey: formResponsesKeys.details(responseId ?? ""),
		queryFn: () => getFormResponseServerFn({ data: responseId ?? "" }),
		enabled: Boolean(responseId),
	});
}
