import { Chip, Switch, Table, toast } from "@heroui/react";
import { Archive, ListChecks, Pencil, Plus, Sparkles } from "lucide-react";
import { useRef, useState } from "react";
import type { SystemField } from "#/api/models";
import { Page } from "#/components/layout";
import { Button, ConfirmDialog, EmptyState } from "#/components/ui";
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
							onPress={() => standard.mutation.mutate(undefined)}
						>
							Add standard fields
						</Button>
					</EmptyState>
				}
			>
				<div className="toolbar">
					<div className="toolbar-left">
						<span className="toolbar-toggle">
							<Switch
								id="system-fields-archived"
								isSelected={includeArchived}
								onChange={(selected) => setIncludeArchived(selected)}
							>
								<Switch.Content>
									<Switch.Control>
										<Switch.Thumb />
									</Switch.Control>
								</Switch.Content>
							</Switch>
							<label htmlFor="system-fields-archived">Include archived</label>
						</span>
					</div>

					<div className="toolbar-right flex gap-2">
						<Button
							variant="secondary"
							icon={<Sparkles size={15} />}
							isPending={standard.mutation.isPending}
							onPress={() => standard.mutation.mutate(undefined)}
						>
							Add standard fields
						</Button>
						<Button
							variant="primary"
							icon={<Plus size={15} />}
							onPress={() => drawerRef.current?.define()}
						>
							New system field
						</Button>
					</div>
				</div>

				<Table variant="secondary" className="table-container">
					<Table.ScrollContainer>
						<Table.Content aria-label="System fields">
							<Table.Header>
								<Table.Column isRowHeader>Label</Table.Column>
								<Table.Column>Code</Table.Column>
								<Table.Column className="table-header-sm">Type</Table.Column>
								<Table.Column>Pre-filled from</Table.Column>
								<Table.Column aria-label="Actions" />
							</Table.Header>
							<Table.Body>
								{fields.map((field) => (
									<Table.Row key={field.systemFieldId}>
										<Table.Cell>
											{field.label}
											{field.isArchived ? (
												<Chip size="sm" className="badge badge-closed ml-2">
													Archived
												</Chip>
											) : null}
										</Table.Cell>
										<Table.Cell className="table-figure">{field.code}</Table.Cell>
										<Table.Cell>{fieldTypes[field.type]}</Table.Cell>
										<Table.Cell>
											{field.source === "None" ? "—" : systemFieldSources[field.source]}
										</Table.Cell>
										<Table.Cell>
											{field.isArchived ? null : (
												<div className="flex justify-end gap-1">
													<Button
														variant="ghost"
														icon={<Pencil size={15} />}
														aria-label="Edit"
														title="Edit"
														onPress={() => drawerRef.current?.edit(field)}
													/>
													<Button
														variant="ghost"
														icon={<Archive size={15} />}
														aria-label="Archive"
														title="Archive"
														onPress={() => setArchiving(field)}
													/>
												</div>
											)}
										</Table.Cell>
									</Table.Row>
								))}
							</Table.Body>
						</Table.Content>
					</Table.ScrollContainer>
				</Table>

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
