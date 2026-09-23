import { useNavigate } from "@tanstack/react-router";
import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import {
	type AddTagCommand,
	AddTagsDrawer,
} from "#/features/applications/pages/components/forms/add-tag";
import type { CandidateProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { EditCandidateDrawer, type EditCandidateFormCommand } from "../form";
import { type CanidateActions, getColumns } from "./CandidatesTableColumns";

interface CandidatesTableProps {
	items: CandidateProjection[];
	onRefresh: () => void;
}

export function CandidatesTable({ items, onRefresh }: CandidatesTableProps) {
	const formRef = useRef<EditCandidateFormCommand>(null);
	const tagRef = useRef<AddTagCommand>(null);

	const actionsHandler: CanidateActions = {
		onEdit: (it) => formRef.current?.edit(it.id),
		onTag: (it) => tagRef.current?.addTag(it.id, it.fullName, "candidate"),
	};

	const navigate = useNavigate();

	const handleRowClick = (value: CandidateProjection) => {
		navigate({
			to: "/app/candidates/$id",
			search: { search: undefined, source: undefined, tab: undefined },
			params: { id: value.id },
		});
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
			<MainTable onRowClick={handleRowClick} table={table} className="table-wide" />
			<EditCandidateDrawer ref={formRef} onSuccess={onRefresh} />
			<AddTagsDrawer ref={tagRef} onSuccess={onRefresh} />
		</>
	);
}
