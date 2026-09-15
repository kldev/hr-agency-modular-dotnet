import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import type { OpportunityProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import type { SalesActionRef, SalesActionTypes } from "../forms";
import SalesActionDrawers from "../forms/SalesActionDrawers";
import { type Actions, getColumns } from "./SalesTableColumns";

interface SalesTableProps {
	items: OpportunityProjection[];
	onRefresh: () => void;
}

export function SalesTable({ items, onRefresh }: SalesTableProps) {
	const salesRef = useRef<SalesActionRef>(null);

	const actionsHandler: Actions = {
		onAction: (action: SalesActionTypes, item: OpportunityProjection): void => {
			salesRef?.current?.onAction(item.id, action);
		},
	};
	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns(actionsHandler),
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
			<MainTable table={table} className="table-wide" />
			<SalesActionDrawers ref={salesRef} onSuccess={onRefresh} />
		</>
	);
}
