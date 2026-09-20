import { FileSignature, Mail, Pencil } from "lucide-react";
import type { EmailPurpose, ProjectProjection } from "@/api/models";
import { Button, ContractStatusBadge, DetailItem, DetailOverviewHeader } from "@/components/ui";
import { formatDate } from "@/utlis/dateUtils";
import { emailPurposeDescriptions, emailPurposes } from "../../../types";

interface ProjectContractSectionProps {
	project: ProjectProjection;
	onRecordContract: () => void;
	onEditEmails: (purpose: EmailPurpose) => void;
}

const purposes: EmailPurpose[] = ["Invoice", "Document"];

export function ProjectContractSection({
	project,
	onRecordContract,
	onEditEmails,
}: ProjectContractSectionProps) {
	const contract = project.contract;

	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Contract"
				description="One contract per project. The other party is frozen as it stood when the contract was recorded."
			/>

			{contract ? (
				<dl className="data-details-list">
					<DetailItem label="Number">{contract.contractNumber}</DetailItem>

					<DetailItem label="Status">
						<ContractStatusBadge status={contract.status} />
					</DetailItem>

					<DetailItem label="Signed on">
						{contract.signedOn ? formatDate(contract.signedOn) : "Not signed"}
					</DetailItem>

					<DetailItem label="Valid">
						{formatDate(contract.validFrom)} –{" "}
						{contract.validTo ? formatDate(contract.validTo) : "open-ended"}
					</DetailItem>

					<DetailItem label="Signed by">
						{contract.signedBy?.fullname ??
							(contract.signedBy
								? `${contract.signedBy.firstName} ${contract.signedBy.lastName}`
								: "—")}
					</DetailItem>

					<DetailItem label="Party">
						{contract.party.legalName}
						{contract.party.taxId ? ` · ${contract.party.taxId}` : ""}
						{contract.party.vatNumber ? ` · ${contract.party.vatNumber}` : ""}
					</DetailItem>

					<DetailItem label="Party address">
						{`${contract.party.registeredAddress.street} ${contract.party.registeredAddress.buildingNumber}, ${contract.party.registeredAddress.postalCode} ${contract.party.registeredAddress.city}, ${contract.party.registeredAddress.countryCode}`}
					</DetailItem>
				</dl>
			) : (
				<p className="project-role-empty">
					No contract recorded yet. A project cannot go live without a signed one.
				</p>
			)}

			<Button
				variant={contract ? "ghost" : "primary"}
				className="mt-3"
				icon={contract ? <Pencil size={15} /> : <FileSignature size={15} />}
				onClick={onRecordContract}
			>
				{contract ? "Update contract" : "Record contract"}
			</Button>

			<div className="project-section-list mt-5">
				{purposes.map((purpose) => {
					const emails = project.emailRecipients
						.filter((recipient) => recipient.purpose === purpose)
						.map((recipient) => recipient.email);

					return (
						<div key={purpose} className="project-role-row">
							<div>
								<div className="project-role-label">{emailPurposes[purpose]} e-mails</div>

								{emails.length > 0 ? (
									<div className="project-email-chips">
										{emails.map((email) => (
											<span key={email} className="badge badge-inactive">
												{email}
											</span>
										))}
									</div>
								) : (
									<div className="project-role-empty">None set</div>
								)}

								<div className="project-role-description">{emailPurposeDescriptions[purpose]}</div>
							</div>

							<Button
								variant="ghost"
								icon={<Mail size={15} />}
								onClick={() => onEditEmails(purpose)}
							>
								Edit
							</Button>
						</div>
					);
				})}
			</div>
		</div>
	);
}
