import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import { CreateOpportunityDrawer, type CreateOpportunityRef } from "#/features/sales/components";
import type { CompanyProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import type { CompanyContactCommand } from "@/features/company-contacts/components/form";
import CompanyContactDrawer from "@/features/company-contacts/components/form/CompanyContactDrawer";
import type { EditCompanyFormCommand } from "../CompanyFormCommand";
import { EditCompanyDrawer } from "../edit";
import { getColumns } from "./CompaniesTableColumns";

interface CompaniesTableProps {
	companies: CompanyProjection[];
	onRefresh: () => void;
}

export function CompaniesTable({ companies, onRefresh }: CompaniesTableProps) {
	const editRef = useRef<EditCompanyFormCommand>(null);
	const contactRef = useRef<CompanyContactCommand>(null);
	const oppRef = useRef<CreateOpportunityRef>(null);

	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns({
				onAddContact: (c) => {
					contactRef?.current?.create(c.id);
				},
				onEdit: (c) => {
					editRef.current?.edit(c.id);
				},
				onAddOpportunity: (c) => {
					oppRef.current?.create({ companyId: c.id, companyName: c.name });
				},
			}),
			data: companies,
			getRowId: (company) => company.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return (
		<>
			<MainTable table={table} className="table-wide" />
			<EditCompanyDrawer
				ref={editRef}
				onSuccess={() => {
					onRefresh();
				}}
			/>
			<CompanyContactDrawer ref={contactRef} onSuccess={() => {}} />
			<CreateOpportunityDrawer ref={oppRef} onSuccess={onRefresh} />
		</>
	);
}
