import { AlertTriangle, Landmark } from "lucide-react";
import type { ProjectProjection } from "@/api/models";
import { Button, DetailItem, DetailOverviewHeader } from "@/components/ui";
import { useGetCompany } from "@/features/companies/pages/hooks";
import { formatAddress } from "../../../utils";

interface ProjectCustomerSectionProps {
	project: ProjectProjection;
	onCompleteProfile: (companyId: string) => void;
	onChangeLegalEntity: () => void;
}

/**
 * Where the project meets the company, and the only such place. The project never edits company
 * data in its own forms - it opens the company's own profile form, so that the answer to "what are
 * this client's registered details" stays in one place.
 */
export function ProjectCustomerSection({
	project,
	onCompleteProfile,
	onChangeLegalEntity,
}: ProjectCustomerSectionProps) {
	const companyQuery = useGetCompany(project.companyId);
	const company = companyQuery.data;
	const profile = company?.profile;

	const address = profile?.registeredAddress;

	// Draft is the only state before a project starts, which is the whole of the rule.
	const isDraft = project.status === "Draft";

	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Client"
				description="The company we invoice, and whose registered details the contract states."
			/>

			<dl className="data-details-list">
				<DetailItem label="Name">{project.companyName}</DetailItem>

				<DetailItem label="Tax ID">{project.companyTaxId}</DetailItem>

				<DetailItem label="Legal name">{profile?.legalName ?? "—"}</DetailItem>

				<DetailItem label="VAT number">{profile?.vatNumber ?? "—"}</DetailItem>

				<DetailItem label="Registered address">{address ? formatAddress(address) : "—"}</DetailItem>

				<DetailItem label="Bank account">{profile?.iban ?? "—"}</DetailItem>
			</dl>

			{company && !company.isProfileComplete ? (
				<div className="project-section-body">
					<div className="project-inline-warning">
						<AlertTriangle size={15} />

						<span>
							The client profile is incomplete. The project cannot go live, and a contract cannot
							record who signed it and under which address, until this is filled in.
						</span>
					</div>

					<Button variant="primary" onClick={() => onCompleteProfile(project.companyId)}>
						Complete client data
					</Button>
				</div>
			) : null}

			<DetailOverviewHeader
				title="Delivered by"
				description="Our company behind this engagement, as it stood when the project was set up."
			/>

			<dl className="data-details-list">
				<DetailItem label="Company">{project.deliveringEntity.name}</DetailItem>

				<DetailItem label="Registered name">{project.deliveringEntity.legalName}</DetailItem>

				<DetailItem label="Tax ID">{project.deliveringEntity.taxId}</DetailItem>

				<DetailItem label="VAT number">{project.deliveringEntity.vatNumber ?? "—"}</DetailItem>

				<DetailItem label="Registered address">
					{formatAddress(project.deliveringEntity.registeredAddress)}
				</DetailItem>
			</dl>

			<div className="project-section-body">
				{isDraft ? (
					<Button variant="ghost" icon={<Landmark size={15} />} onClick={onChangeLegalEntity}>
						Change company
					</Button>
				) : (
					<p className="project-role-description">
						The project has started, so this is settled. Carrying on under another company means
						copying the project.
					</p>
				)}
			</div>
		</div>
	);
}
