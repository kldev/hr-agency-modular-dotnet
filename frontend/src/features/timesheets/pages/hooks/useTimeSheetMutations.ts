import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import {
	approveTimeSheet,
	commentOnTimeSheet,
	removeWorkDay,
	returnTimeSheetForCorrection,
	saveWorkDay,
	settleTimeSheet,
	submitTimeSheet,
} from "@/api/endpoints";
import type {
	ApproveTimeSheetRequest,
	CommentOnTimeSheetRequest,
	ReturnTimeSheetRequest,
	SaveWorkDayRequest,
	TimeSheetApproved,
	TimeSheetCommented,
	TimeSheetReturnedForCorrection,
	TimeSheetSettled,
	TimeSheetSubmitted,
	WorkDayRemoved,
	WorkDaySaved,
} from "@/api/models";
import { timeSheetsKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

type MutationOptions<TResult = unknown> = {
	onSuccess: (result: TResult) => void;

	/* Only for writes fired from a confirmation dialog; a drawer shows the refusal inline. */
	onError?: (error: unknown) => void;
};

/** Which sheet a decision is about. The route takes all three, and so does every command here. */
export type SheetRef = { userId: string; year: number; month: number };

const saveWorkDayServerFn = createServerFn({ method: "POST" })
	.validator((input: { req: SaveWorkDayRequest }) => input)
	.handler(async ({ data }) => saveWorkDay(data.req, await getFnOptions()));

const removeWorkDayServerFn = createServerFn({ method: "POST" })
	.validator((input: string) => input)
	.handler(async ({ data }) => removeWorkDay(data, await getFnOptions()));

const submitServerFn = createServerFn({ method: "POST" })
	.validator((input: { year: number; month: number; comment: string | null }) => input)
	.handler(async ({ data }) =>
		submitTimeSheet(
			{ year: data.year, month: data.month, comment: data.comment },
			await getFnOptions(),
		),
	);

const approveServerFn = createServerFn({ method: "POST" })
	.validator((input: { sheet: SheetRef; req: ApproveTimeSheetRequest }) => input)
	.handler(async ({ data }) =>
		approveTimeSheet(
			data.sheet.userId,
			data.sheet.year,
			data.sheet.month,
			data.req,
			await getFnOptions(),
		),
	);

const returnServerFn = createServerFn({ method: "POST" })
	.validator((input: { sheet: SheetRef; req: ReturnTimeSheetRequest }) => input)
	.handler(async ({ data }) =>
		returnTimeSheetForCorrection(
			data.sheet.userId,
			data.sheet.year,
			data.sheet.month,
			data.req,
			await getFnOptions(),
		),
	);

const settleServerFn = createServerFn({ method: "POST" })
	.validator((input: SheetRef) => input)
	.handler(async ({ data }) =>
		settleTimeSheet(data.userId, data.year, data.month, await getFnOptions()),
	);

const commentServerFn = createServerFn({ method: "POST" })
	.validator((input: { sheet: SheetRef; req: CommentOnTimeSheetRequest }) => input)
	.handler(async ({ data }) =>
		commentOnTimeSheet(
			data.sheet.userId,
			data.sheet.year,
			data.sheet.month,
			data.req,
			await getFnOptions(),
		),
	);

/*
 * One key for the whole area, invalidated by every write, and the wait is not decoration: the
 * projection is rebuilt by an async daemon. A month is read by four different screens - my
 * calendar, the monitoring list, the approval queue and the settlement list - and a decision on
 * one sheet moves it between them, so invalidating anything narrower would leave two of them
 * showing the answer from before.
 */
function useTimeSheetMutation<TVariables, TResult>(
	mutationFn: (variables: TVariables) => Promise<TResult>,
	{ onSuccess, onError }: MutationOptions<TResult>,
) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn,

		onSuccess: async (result) => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: timeSheetsKeys.all });

			onSuccess(result);
		},

		onError,
	});

	return { mutation, waiting };
}

export function useSaveWorkDay(options: MutationOptions<WorkDaySaved>) {
	return useTimeSheetMutation(
		({ request }: { request: SaveWorkDayRequest }) =>
			saveWorkDayServerFn({ data: { req: request } }),
		options,
	);
}

export function useRemoveWorkDay(options: MutationOptions<WorkDayRemoved>) {
	return useTimeSheetMutation(
		({ date }: { date: string }) => removeWorkDayServerFn({ data: date }),
		options,
	);
}

export function useSubmitTimeSheet(options: MutationOptions<TimeSheetSubmitted>) {
	return useTimeSheetMutation(
		({ year, month, comment }: { year: number; month: number; comment: string | null }) =>
			submitServerFn({ data: { year, month, comment } }),
		options,
	);
}

export function useApproveTimeSheet(options: MutationOptions<TimeSheetApproved>) {
	return useTimeSheetMutation(
		({ sheet, request }: { sheet: SheetRef; request: ApproveTimeSheetRequest }) =>
			approveServerFn({ data: { sheet, req: request } }),
		options,
	);
}

export function useReturnTimeSheet(options: MutationOptions<TimeSheetReturnedForCorrection>) {
	return useTimeSheetMutation(
		({ sheet, request }: { sheet: SheetRef; request: ReturnTimeSheetRequest }) =>
			returnServerFn({ data: { sheet, req: request } }),
		options,
	);
}

export function useSettleTimeSheet(options: MutationOptions<TimeSheetSettled>) {
	return useTimeSheetMutation(
		({ sheet }: { sheet: SheetRef }) => settleServerFn({ data: sheet }),
		options,
	);
}

export function useCommentOnTimeSheet(options: MutationOptions<TimeSheetCommented>) {
	return useTimeSheetMutation(
		({ sheet, request }: { sheet: SheetRef; request: CommentOnTimeSheetRequest }) =>
			commentServerFn({ data: { sheet, req: request } }),
		options,
	);
}
