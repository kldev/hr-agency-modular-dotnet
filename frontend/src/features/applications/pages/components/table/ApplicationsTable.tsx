import { useNavigate } from "@tanstack/react-router";
import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import {
	type WorkerWizardCommand,
	WorkerWizardDialog,
} from "#/features/workers/wizards/worker/WorkerWizardDialog";
import type { JobApplicationProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { ApplicationsActionDrawers, type JobApplicationsRef } from "../forms";
import { type Actions, getColumns } from "./ApplicationsTableColumns";

interface AplicationsTableProps {
	items: JobApplicationProjection[];
	onRefresh: () => void;
}

export function ApplicationsTable({ items, onRefresh }: AplicationsTableProps) {
	const formRef = useRef<JobApplicationsRef>(null);
	const workerRef = useRef<WorkerWizardCommand>(null);

	const handleActions: Actions = {
		onAction: (action, item) => {
			/*
			 * Taking somebody on is not an application action - it opens the worker register's own
			 * wizard, with the application preselected so their name and contact details come along.
			 */
			if (action === "register-worker") {
				workerRef.current?.register(item.id);
				return;
			}

			formRef.current?.update(item.id, action, item.status, {
				fullName: item.applicantFullName,
				email: item.applicantEmail,
			});
		},
	};
	const navigate = useNavigate();

	const handleRowClick = (value: JobApplicationProjection) => {
		navigate({
			to: "/app/applications/$id",
			search: { status: undefined, search: undefined, source: undefined },
			params: { id: value.id },
		});
	};

	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns(handleActions),
			data: items,
			getRowId: (user) => user.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return (
		<>
			<MainTable table={table} className="table-wide" onRowClick={handleRowClick} />
			<ApplicationsActionDrawers ref={formRef} onSuccess={onRefresh} />

			<WorkerWizardDialog
				ref={workerRef}
				onSuccess={(workerId) => {
					navigate({
						to: "/app/workers/$id",
						params: { id: workerId },
						search: { search: undefined, tab: undefined },
					});
				}}
			/>
		</>
	);
}
