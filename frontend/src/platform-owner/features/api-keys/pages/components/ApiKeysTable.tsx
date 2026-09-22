import { createColumnHelper, useTable } from "@tanstack/react-table";
import { Ban } from "lucide-react";
import type { ServiceApiKeyRow } from "#/api/models";
import type { appTableFeaturesType } from "@/components/table";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { ActionMenu } from "@/components/ui/ActionMenu";
import { formatDateTime } from "@/utlis/dateUtils";

const columnHelper = createColumnHelper<appTableFeaturesType, ServiceApiKeyRow>();

interface Props {
	keys: ServiceApiKeyRow[];
	onRevoke: (key: ServiceApiKeyRow) => void;
}

export function KeyState({ apiKey }: { apiKey: ServiceApiKeyRow }) {
	return apiKey.revokedAt ? (
		<span className="badge badge-closed" title={`Revoked ${formatDateTime(apiKey.revokedAt)}`}>
			Revoked
		</span>
	) : (
		<span className="badge badge-active">Active</span>
	);
}

/** Only the prefix is ever shown - the value was never kept. */
export function KeyPrefix({ apiKey }: { apiKey: ServiceApiKeyRow }) {
	return <code className="table-number">{apiKey.displayPrefix}…</code>;
}

export function ApiKeysTable({ keys, onRevoke }: Props) {
	const table = useTable(
		{
			features: appTableFeatures,
			columns: columnHelper.columns([
				columnHelper.display({
					id: "actions",
					header: () => null,
					meta: { width: "xxs" },
					cell: ({ row }) =>
						// A revoked key stays listed for its history and has nothing left to do.
						row.original.revokedAt ? null : (
							<div className="table-actions">
								<ActionMenu
									actions={[{ label: "Revoke", icon: Ban, action: () => onRevoke(row.original) }]}
								/>
							</div>
						),
				}),
				columnHelper.accessor("name", {
					header: "Name",
					meta: { width: "xl" },
					cell: ({ getValue }) => <span className="truncate">{getValue()}</span>,
				}),
				columnHelper.accessor("displayPrefix", {
					header: "Key",
					meta: { width: "md" },
					cell: ({ row }) => <KeyPrefix apiKey={row.original} />,
				}),
				columnHelper.accessor("createdAt", {
					header: "Issued",
					meta: { width: "md" },
					cell: ({ getValue }) => <span>{formatDateTime(getValue())}</span>,
				}),
				columnHelper.display({
					id: "state",
					header: "State",
					meta: { width: "sm" },
					cell: ({ row }) => <KeyState apiKey={row.original} />,
				}),
			]),
			data: keys,
			getRowId: (key) => key.id,
			enableSorting: false,
		},
		(state) => ({ sorting: state.sorting }),
	);

	return <MainTable table={table} />;
}
