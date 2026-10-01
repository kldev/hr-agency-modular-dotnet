import { Table } from "@heroui/react";
import { useParams } from "@tanstack/react-router";
import { useRef } from "react";
import {
	AuditInformation,
	DetailsHeader,
	DetailsLoading,
	LegalEntityStatusBadge,
} from "@/components/ui";
import {
	DataDetails,
	DataDetailsLayout,
	DetailItem,
	DetailOverviewHeader,
	EmailItem,
} from "@/components/ui/details/DataDetails";
import { formatDate } from "@/utlis/dateUtils";
import { type CloseLegalEntityCommand, CloseLegalEntityDrawer } from "../drawers";
import { bankAccountPurposes, currencyCodes, isTrading } from "../types";
import {
	type LegalEntityFormCommand,
	LegalEntityWizardDialog,
} from "../wizards/legal-entity/LegalEntityWizardDialog";
import { LegalEntityActions } from "./components";
import { useGetLegalEntity } from "./hooks";

export function LegalEntityDetailsPage() {
	const { id } = useParams({ from: "/app/legal-entities/$id" });

	const formRef = useRef<LegalEntityFormCommand>(null);
	const closeRef = useRef<CloseLegalEntityCommand>(null);

	const query = useGetLegalEntity(id);

	if (!id || query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !query.data} />
		);
	}

	const entity = query.data;
	const trading = isTrading(entity.activeFrom, entity.activeTo);

	const refresh = () => {
		void query.refetch();
	};

	return (
		<>
			<DataDetails>
				<DetailsHeader
					name={entity.name}
					onEdit={() => formRef.current?.edit(entity)}
					detailsAddons={<LegalEntityStatusBadge isTrading={trading} />}
					extraAdd={
						<LegalEntityActions
							mode="details"
							id={entity.id}
							name={entity.name}
							isClosed={Boolean(entity.activeTo)}
							onEdit={() => formRef.current?.edit(entity)}
							onClose={() => closeRef.current?.close(entity)}
						/>
					}
				/>

				<DataDetailsLayout
					main={
						<>
							<section className="data-details-section">
								<div className="data-overview">
									<DetailOverviewHeader
										title="Registered details"
										description="What goes on a contract and on an invoice."
									/>

									<dl className="data-details-list">
										<DetailItem label="Registered name">{entity.legalName}</DetailItem>

										<DetailItem label="Tax ID">{entity.taxId}</DetailItem>

										<DetailItem label="VAT number">{entity.vatNumber || "—"}</DetailItem>

										<DetailItem label="Registered address">
											{entity.registeredAddress.street} {entity.registeredAddress.buildingNumber}
											{entity.registeredAddress.unitNumber
												? `/${entity.registeredAddress.unitNumber}`
												: ""}
											, {entity.registeredAddress.postalCode} {entity.registeredAddress.city},{" "}
											{entity.registeredAddress.countryCode.toUpperCase()}
										</DetailItem>

										<DetailItem label="Trading since">{formatDate(entity.activeFrom)}</DetailItem>

										<DetailItem label="Trading until">
											{entity.activeTo ? formatDate(entity.activeTo) : "Still trading"}
										</DetailItem>
									</dl>
								</div>
							</section>

							<section className="data-details-section">
								<div className="data-overview">
									<DetailOverviewHeader
										title="President"
										description="Who the register names as running this company."
									/>

									<dl className="data-details-list">
										<DetailItem label="Name">
											{entity.president.firstName} {entity.president.lastName}
										</DetailItem>

										{entity.president.email ? (
											<EmailItem email={entity.president.email} />
										) : (
											<DetailItem label="Email">—</DetailItem>
										)}
									</dl>
								</div>
							</section>

							<section className="data-details-section">
								<div className="data-overview">
									<DetailOverviewHeader
										title="Bank accounts"
										description="One per purpose and currency. The incoming one is what an invoice quotes."
									/>

									{entity.bankAccounts.length === 0 ? (
										<div className="legal-entity-section-body">
											<p className="data-details-empty">No accounts recorded.</p>
										</div>
									) : (
										<Table variant="secondary" className="table-container">
											<Table.ScrollContainer>
												<Table.Content aria-label="Bank accounts">
													<Table.Header>
														<Table.Column isRowHeader>Purpose</Table.Column>
														<Table.Column>Currency</Table.Column>
														<Table.Column>IBAN</Table.Column>
														<Table.Column>BIC</Table.Column>
														<Table.Column>Bank</Table.Column>
													</Table.Header>

													<Table.Body>
														{entity.bankAccounts.map((account) => (
															<Table.Row key={`${account.purpose}-${account.currency}`}>
																<Table.Cell>{bankAccountPurposes[account.purpose]}</Table.Cell>
																<Table.Cell>{currencyCodes[account.currency]}</Table.Cell>
																<Table.Cell className="table-cell-truncate">
																	<span title={account.iban}>{account.iban}</span>
																</Table.Cell>
																<Table.Cell>{account.bic || "—"}</Table.Cell>
																<Table.Cell className="table-cell-truncate">
																	<span title={account.bankName ?? ""}>
																		{account.bankName || "—"}
																	</span>
																</Table.Cell>
															</Table.Row>
														))}
													</Table.Body>
												</Table.Content>
											</Table.ScrollContainer>
										</Table>
									)}
								</div>
							</section>

							{entity.description ? (
								<section className="data-details-section">
									<div className="data-overview">
										<DetailOverviewHeader title="Description" description="Free notes." />

										<div className="legal-entity-section-body">
											<p className="data-details-body">{entity.description}</p>
										</div>
									</div>
								</section>
							) : null}
						</>
					}
					sidebar={
						<AuditInformation
							createdAt={entity.createdAt}
							createdBy={entity.createdBy}
							modifiedAt={entity.modifiedAt}
							modifiedBy={entity.modifiedBy}
						/>
					}
				/>
			</DataDetails>

			<LegalEntityWizardDialog ref={formRef} onSuccess={refresh} />
			<CloseLegalEntityDrawer ref={closeRef} onSuccess={refresh} />
		</>
	);
}
