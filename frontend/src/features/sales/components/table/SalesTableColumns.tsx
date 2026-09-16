import { createColumnHelper } from "@tanstack/react-table";
import { MessagePreview } from "#/components/ui/MessagePreview";
import { formatSalary } from "#/utlis";
import type { OpportunityProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark, OpportunityStageBadge } from "@/components/ui";
import { formatDate, formatDateTime } from "@/utlis/dateUtils";
import type { SalesActionTypes } from "../forms";
import { SalesStageBadge } from "../sales-stage-badge";
import { SalesActions } from "./SalesActions";

export type Actions = {
	onAction: (action: SalesActionTypes, item: OpportunityProjection) => void;
};

const columnHelper = createColumnHelper<appTableFeaturesType, OpportunityProjection>();

export function getColumns(actions: Actions) {
	const columns = columnHelper.columns([
		columnHelper.display({
			id: "actions",
			header: () => null,
			meta: {
				width: "xxs",
			},
			cell: ({ row }) => {
				const item = row.original;

				return (
					<div className="table-cell-content">
						<SalesActions
							onAction={(val) => actions.onAction(val, item)}
							mode="table"
							opportunityId={item.id}
						/>
					</div>
				);
			},
		}),
		columnHelper.accessor("title", {
			header: "Title",
			meta: {
				width: "2xl",
			},

			cell: ({ row }) => (
				<div className="table-cell-content">
					<ItemMark name={row.original.title} />
					<div className="data-meta truncate">{row.original.title}</div>
				</div>
			),
		}),
		columnHelper.accessor("responsible", {
			header: "Responsible",
			meta: {
				width: "md",
			},
			cell: ({ row }) => (
				<div className="table-cell-content">
					<div className="min-w-0">
						<div className="data-name">{row.original.responsible.fullname ?? ""}</div>

						<div className="data-meta">{row.original.responsible.email}</div>
					</div>
				</div>
			),
		}),
		columnHelper.accessor("stage", {
			header: "Stage",
			meta: {
				width: "sm",
			},
			cell: ({ getValue }) => <SalesStageBadge stage={getValue()} />,
		}),
		columnHelper.accessor("expectedValue", {
			header: "Value",
			meta: {
				width: "sm",
			},
			cell: ({ row, getValue }) => (
				<span className="table-number font-medium">
					{formatSalary(getValue() as number)} {row.original.currencyCode}
				</span>
			),
		}),
		columnHelper.accessor("expectedCloseDate", {
			header: "Expected close date",
			meta: {
				width: "sm",
			},
			cell: ({ getValue }) => <span className="table-number">{formatDate(getValue())}</span>,
		}),

		columnHelper.accessor("company.name", {
			header: "Company",
			meta: {
				width: "md",
			},
			cell: ({ getValue }) => <div className="data-meta truncate">{getValue()}</div>,
		}),

		columnHelper.accessor("description", {
			header: "Description",
			cell: ({ getValue }) => <MessagePreview message={getValue()} />,
			meta: {
				width: "2xl",
			},
		}),
		columnHelper.accessor("createdAt", {
			header: "Created at",
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),
	]);
	return columns;
}
