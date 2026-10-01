import { Table } from "@heroui/react";
import { ClipboardList } from "lucide-react";
import { useRef } from "react";
import { Button, DetailOverviewHeader, EmptyState, FormResponseStatusBadge } from "#/components/ui";
import { formatDateTime } from "#/utlis/dateUtils";
import { useGetWorkerFormResponses } from "../hooks";
import { FormResponseDialog, type FormResponseDialogCommand } from "./FormResponseDialog";
import { type StartFormCommand, StartFormDrawer } from "./StartFormDrawer";

/**
 * The forms filled in for one worker - consents, statements, surveys - on their own tab of the
 * worker's page. The tab owns its drawer and dialog: nothing else on the page opens them.
 */
export function WorkerFormsSection({ workerId }: { workerId: string }) {
	const startRef = useRef<StartFormCommand>(null);
	const responseRef = useRef<FormResponseDialogCommand>(null);

	const query = useGetWorkerFormResponses(workerId);
	const responses = query.data ?? [];

	return (
		<>
			<div className="data-overview">
				<DetailOverviewHeader
					title="Forms"
					description="Documents and surveys filled in for this person. A submitted one keeps the version it was given to."
					onAdd={() => startRef.current?.start(workerId)}
				/>

				{query.isFetched && responses.length === 0 ? (
					<EmptyState title="No forms yet" description="Start one with the button above.">
						<ClipboardList size={24} />
					</EmptyState>
				) : (
					<Table variant="secondary" className="table-container">
						<Table.ScrollContainer>
							<Table.Content aria-label="Forms">
								<Table.Header>
									<Table.Column isRowHeader>Form</Table.Column>
									<Table.Column className="table-header-sm">Version</Table.Column>
									<Table.Column className="table-header-sm">Status</Table.Column>
									<Table.Column>Last change</Table.Column>
									<Table.Column aria-label="Actions" />
								</Table.Header>

								<Table.Body>
									{responses.map((response) => (
										<Table.Row key={response.id}>
											<Table.Cell>{response.formName}</Table.Cell>
											<Table.Cell className="table-figure">
												v{String(response.formVersion)}
												{Number(response.revision) > 0
													? ` · rev. ${String(response.revision)}`
													: ""}
											</Table.Cell>
											<Table.Cell>
												<FormResponseStatusBadge status={response.status} />
											</Table.Cell>
											<Table.Cell>
												{formatDateTime(response.modifiedAt ?? response.startedAt)}
												{response.modifiedBy ? ` · ${response.modifiedBy.fullname}` : ""}
											</Table.Cell>
											<Table.Cell>
												<div className="flex justify-end">
													<Button
														variant="secondary"
														onPress={() => responseRef.current?.open(response.id)}
													>
														{response.status === "Draft" ? "Continue" : "Open"}
													</Button>
												</div>
											</Table.Cell>
										</Table.Row>
									))}
								</Table.Body>
							</Table.Content>
						</Table.ScrollContainer>
					</Table>
				)}
			</div>

			<StartFormDrawer ref={startRef} onStarted={(id) => responseRef.current?.open(id)} />
			<FormResponseDialog ref={responseRef} onChanged={() => void query.refetch()} />
		</>
	);
}
