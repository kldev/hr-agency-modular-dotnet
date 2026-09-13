import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import type { JobPostResponse } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import {
	type CreateJobApplicationsCommand,
	CreateJobApplicationsDrawer,
} from "@/features/applications/pages/components";
import {
	type ChangeJobPostStatusCommand,
	ChangeJobPostStatusDrawer,
	type PostToChannelCommand,
	PostToChannelDrawer,
} from "../forms";
import { type Actions, getColumns } from "./JobPostsTableColumns";

interface JobPostsTableProps {
	items: JobPostResponse[];
	onRefresh: () => void;
}

export function JobPostsTable({ items, onRefresh }: JobPostsTableProps) {
	const applicationRef = useRef<CreateJobApplicationsCommand>(null);
	const statusRef = useRef<ChangeJobPostStatusCommand>(null);
	const channelRef = useRef<PostToChannelCommand>(null);

	const actionsHandler: Actions = {
		onAddApplication: (it) => {
			applicationRef.current?.create(it.id, it.title);
		},
		onChangeStatus: (it) => {
			statusRef.current?.changeStatus(it.id);
		},
		onPostToChannel: (it) => {
			channelRef.current?.postToChannel(it.id);
		},
	};

	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns(actionsHandler),
			data: items,
			getRowId: (item) => item.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return (
		<>
			<MainTable table={table} className="table-wide" />
			<CreateJobApplicationsDrawer ref={applicationRef} onSuccess={onRefresh} />
			<PostToChannelDrawer ref={channelRef} onSuccess={onRefresh} />
			<ChangeJobPostStatusDrawer ref={statusRef} onSuccess={onRefresh} />
		</>
	);
}
