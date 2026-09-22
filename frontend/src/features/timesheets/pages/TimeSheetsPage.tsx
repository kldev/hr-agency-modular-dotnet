import { CalendarClock } from "lucide-react";
import { useRef, useState } from "react";
import { toast } from "sonner";
import { Route } from "#/routes/app/timesheets";
import { useAuthStore } from "#/stores/authStore";
import type { TimeSheetProjection } from "@/api/models";
import { Page } from "@/components/layout";
import { ConfirmDialog, EmptyState, type TabDefinition, TabPanel, Tabs } from "@/components/ui";
import {
	ApproveTimeSheetDrawer,
	type ApproveTimeSheetFormCommand,
	CommentOnTimeSheetDrawer,
	type CommentOnTimeSheetFormCommand,
	ReturnTimeSheetDrawer,
	type ReturnTimeSheetFormCommand,
} from "../drawers";
import { monthFromSearch, type TimeSheetTab } from "../searchParams";
import { formatMinutes, isPayroll, type MonthInView, monthLabel } from "../types";
import { MonthNavigator, MyMonthPanel, TeamMonitoringTable, TwoPaneSheets } from "./components";
import {
	useGetMyTimeSheet,
	useGetTeamTimeSheets,
	useGetTimeSheetsForSettlement,
	useSettleTimeSheet,
} from "./hooks";
import "./timesheets.css";

export function TimeSheetsPage() {
	const navigate = Route.useNavigate();
	const search = Route.useSearch();

	const user = useAuthStore((state) => state.user);

	const approveRef = useRef<ApproveTimeSheetFormCommand>(null);
	const returnRef = useRef<ReturnTimeSheetFormCommand>(null);
	const commentRef = useRef<CommentOnTimeSheetFormCommand>(null);

	const [toSettle, setToSettle] = useState<TimeSheetProjection | null>(null);

	const month = monthFromSearch(search);

	const payroll = isPayroll(user?.role);

	const mine = useGetMyTimeSheet(month);
	const team = useGetTeamTimeSheets(month);
	const settlement = useGetTimeSheetsForSettlement(month, payroll);

	const { mutation: settle, waiting } = useSettleTimeSheet({
		onSuccess: () => {
			setToSettle(null);
		},

		onError: () => {
			setToSettle(null);
			toast.error("The month could not be settled");
		},
	});

	const teamRows = team.data ?? [];
	const submitted = teamRows.filter((row) => row.status === "Submitted");

	/*
	 * A tab that does not apply does not exist, rather than sitting there greyed out. "Team" and
	 * "Approvals" appear because the chart gives this person somebody to answer for - not because
	 * of a role, which is the same line the backend draws. "Settlement" is the one role mirror.
	 */
	const tabs: TabDefinition<TimeSheetTab>[] = [{ id: "mine", label: "My hours" }];

	if (teamRows.length > 0) {
		tabs.push({ id: "team", label: "Team", count: teamRows.length });
		tabs.push({ id: "approvals", label: "Approvals", count: submitted.length });
	}

	if (payroll) tabs.push({ id: "settlement", label: "Settlement" });

	const active = tabs.some((tab) => tab.id === search.tab) ? (search.tab as TimeSheetTab) : "mine";

	const setMonth = (next: MonthInView) => {
		navigate({ search: (previous) => ({ ...previous, year: next.year, month: next.month }) });
	};

	const setTab = (tab: TimeSheetTab) => {
		navigate({ search: (previous) => ({ ...previous, tab, person: undefined }) });
	};

	const setPerson = (person: string) => {
		navigate({ search: (previous) => ({ ...previous, person }) });
	};

	const refresh = () => {
		void mine.refetch();
		void team.refetch();

		if (payroll) void settlement.refetch();
	};

	const onApprove = (sheet: TimeSheetProjection) => approveRef.current?.approve(sheet);
	const onReturn = (sheet: TimeSheetProjection) => returnRef.current?.returnForCorrection(sheet);
	const onComment = (sheet: TimeSheetProjection) => commentRef.current?.comment(sheet);

	return (
		<>
			<Page
				title="Time sheets"
				description="Hours recorded month by month: your own, your people's, and the months waiting on a decision."
				loading={mine.isPending || team.isPending}
				emptyState={null}
				headerAddon={
					<MonthNavigator
						month={month}
						onChange={setMonth}
						onRefresh={refresh}
						loading={mine.isFetching || team.isFetching}
					/>
				}
			>
				<Tabs value={active} tabs={tabs} onChange={setTab} label="Time sheet views" />

				{active === "mine" ? (
					<TabPanel id="mine">
						<MyMonthPanel
							month={month}
							userId={user?.userId ?? ""}
							sheet={mine.data ?? null}
							loading={mine.isPending}
							onChanged={refresh}
						/>
					</TabPanel>
				) : null}

				{active === "team" ? (
					<TabPanel id="team">
						{teamRows.length === 0 ? (
							<EmptyState
								title="Nobody to chase"
								description="Nobody below you in the chart owes hours for this month."
							>
								<CalendarClock size={24} />
							</EmptyState>
						) : (
							<TeamMonitoringTable rows={teamRows} month={month} />
						)}
					</TabPanel>
				) : null}

				{active === "approvals" ? (
					<TabPanel id="approvals">
						<TwoPaneSheets
							month={month}
							canSettle={payroll}
							selected={search.person}
							onSelect={setPerson}
							entries={submitted.map((row) => ({
								userId: row.userId,
								name: `${row.user.firstName} ${row.user.lastName}`,
								meta: `${formatMinutes(Number(row.totalMinutes))} · ${Number(row.filledDays)} days`,
								status: "Submitted" as const,
							}))}
							emptyTitle="Nothing waiting"
							emptyDescription={`Nobody has sent ${monthLabel(month)} for approval yet. The team tab is where the chasing happens.`}
							onApprove={onApprove}
							onReturn={onReturn}
							onSettle={setToSettle}
							onComment={onComment}
						/>
					</TabPanel>
				) : null}

				{active === "settlement" ? (
					<TabPanel id="settlement">
						<TwoPaneSheets
							month={month}
							canSettle
							selected={search.person}
							onSelect={setPerson}
							entries={(settlement.data ?? []).map((sheet) => ({
								userId: sheet.userId,
								name: `${sheet.user.firstName} ${sheet.user.lastName}`,
								meta: `${formatMinutes(Number(sheet.totalMinutes ?? 0))} · ${Number(sheet.filledDays ?? 0)} days`,
								status: sheet.status,
							}))}
							emptyTitle="Nothing to settle"
							emptyDescription={`No month of ${monthLabel(month)} has been approved yet, so there is nothing to hand to payroll.`}
							onApprove={onApprove}
							onReturn={onReturn}
							onSettle={setToSettle}
							onComment={onComment}
						/>
					</TabPanel>
				) : null}
			</Page>

			{/* Settling takes no fields at all, so it is a confirmation rather than a drawer. */}
			<ConfirmDialog
				open={Boolean(toSettle)}
				danger={false}
				title="Hand this month to payroll?"
				description={
					toSettle
						? `${toSettle.user.firstName} ${toSettle.user.lastName}, ${monthLabel(month)}: ${formatMinutes(Number(toSettle.totalMinutes ?? 0))}. A settled month is closed - sending it back for correction is the only way out.`
						: ""
				}
				confirmLabel="Settle"
				loading={settle.isPending || waiting}
				onConfirm={() =>
					toSettle &&
					settle.mutate({
						sheet: {
							userId: toSettle.userId,
							year: Number(toSettle.year),
							month: Number(toSettle.month),
						},
					})
				}
				onClose={() => setToSettle(null)}
			/>

			<ApproveTimeSheetDrawer ref={approveRef} onSuccess={refresh} />
			<ReturnTimeSheetDrawer ref={returnRef} onSuccess={refresh} />
			<CommentOnTimeSheetDrawer ref={commentRef} onSuccess={refresh} />
		</>
	);
}
