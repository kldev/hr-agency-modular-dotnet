import { Archive, ListChecks, Pencil, Plus, Sparkles } from "lucide-react";
import { useRef, useState } from "react";
import { toast } from "sonner";
import type { SystemField } from "#/api/models";
import { Page } from "#/components/layout";
import { Button, ConfirmDialog, EmptyState } from "#/components/ui";
import { Toggle } from "#/components/ui/Toggle";
import { type SystemFieldCommand, SystemFieldDrawer } from "../drawers/SystemFieldDrawer";
import { useAddStandardSystemFields, useArchiveSystemField, useGetSystemFields } from "../hooks";
import { fieldTypes, systemFieldSources } from "../types";

/**
 * The catalogue of fields that describe a person, shared by every form. Defined here once, picked
 * in the builder, never copied: five forms asking for a phone number show one value five times.
 */
export default function SystemFieldsPage() {
	const [includeArchived, setIncludeArchived] = useState(false);
	const [archiving, setArchiving] = useState<SystemField | null>(null);
	const drawerRef = useRef<SystemFieldCommand>(null);

	const query = useGetSystemFields(includeArchived);
	const refresh = () => void query.refetch();

	const standard = useAddStandardSystemFields({
		onSuccess: (result) => {
			const added = Number((result as { added?: number | string }).added ?? 0);
			toast.success(
				added ? `Added ${added} standard fields` : "Every standard field is already here",
			);
		},
	});

	const archive = useArchiveSystemField({ onSuccess: () => setArchiving(null) });

	const fields = query.data ?? [];

	return (
		<>
			<Page
				title="System fields"
				description="What forms may ask about a person. Code and type never change; a published form keeps the definition it went out with."
				onRefresh={refresh}
				loading={query.isPending}
				isEmpty={query.isFetched && fields.length === 0 && !includeArchived}
				emptyState={
					<EmptyState
						title="The catalogue is empty"
						description="Start with the standard fields - six of them fill themselves from the worker's file."
					>
						<Button
							variant="primary"
							icon={<Sparkles size={15} />}
							onClick={() => standard.mutation.mutate(undefined)}
						>
							Add standard fields
						</Button>
					</EmptyState>
				}
			>
				<div className="toolbar">
					<div className="toolbar-left">
						<span className="toolbar-toggle">
							<Toggle
								id="system-fields-archived"
								checked={includeArchived}
								onChange={(event) => setIncludeArchived(event.target.checked)}
							/>
							<label htmlFor="system-fields-archived">Include archived</label>
						</span>
					</div>

					<div className="toolbar-right flex gap-2">
						<Button
							variant="secondary"
							icon={<Sparkles size={15} />}
							loading={standard.mutation.isPending}
							onClick={() => standard.mutation.mutate(undefined)}
						>
							Add standard fields
						</Button>
						<Button
							variant="primary"
							icon={<Plus size={15} />}
							onClick={() => drawerRef.current?.define()}
						>
							New system field
						</Button>
					</div>
				</div>

				<table className="table">
					<thead>
						<tr>
							<th>Label</th>
							<th>Code</th>
							<th className="table-header-sm">Type</th>
							<th>Pre-filled from</th>
							<th />
						</tr>
					</thead>
					<tbody>
						{fields.map((field) => (
							<tr key={field.systemFieldId}>
								<td>
									{field.label}
									{field.isArchived ? (
										<span className="badge badge-closed ml-2">Archived</span>
									) : null}
								</td>
								<td className="table-figure">{field.code}</td>
								<td>{fieldTypes[field.type]}</td>
								<td>{field.source === "None" ? "—" : systemFieldSources[field.source]}</td>
								<td>
									{field.isArchived ? null : (
										<div className="flex justify-end gap-1">
											<Button
												variant="ghost"
												icon={<Pencil size={15} />}
												aria-label="Edit"
												title="Edit"
												onClick={() => drawerRef.current?.edit(field)}
											/>
											<Button
												variant="ghost"
												icon={<Archive size={15} />}
												aria-label="Archive"
												title="Archive"
												onClick={() => setArchiving(field)}
											/>
										</div>
									)}
								</td>
							</tr>
						))}
					</tbody>
				</table>

				{fields.length === 0 && includeArchived ? (
					<EmptyState title="Nothing here">
						<ListChecks size={24} />
					</EmptyState>
				) : null}
			</Page>

			<SystemFieldDrawer ref={drawerRef} onSaved={refresh} />

			<ConfirmDialog
				open={Boolean(archiving)}
				title="Archive this system field?"
				description={`${archiving?.label ?? ""} can no longer be added to a form. Published versions keep it, and every value given stays.`}
				confirmLabel="Archive"
				onConfirm={() => archiving && archive.mutation.mutate(archiving.systemFieldId)}
				onClose={() => setArchiving(null)}
			/>
		</>
	);
}
