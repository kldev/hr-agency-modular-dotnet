import { AlertTriangle } from "lucide-react";
import type { ProjectProjection } from "@/api/models";
import { Button, DetailItem, DetailOverviewHeader } from "@/components/ui";
import { useGetCompany } from "@/features/companies/pages/hooks";

interface ProjectCustomerSectionProps {
	project: ProjectProjection;
	onCompleteProfile: (companyId: string) => void;
}

/**
 * Where the project meets the company, and the only such place. The project never edits company
 * data in its own forms - it opens the company's own profile form, so that the answer to "what are
 * this client's registered details" stays in one place.
 */
export function ProjectCustomerSection({
	project,
	onCompleteProfile,
}: ProjectCustomerSectionProps) {
	const companyQuery = useGetCompany(project.companyId);
	const company = companyQuery.data;
	const profile = company?.profile;

	const address = profile?.registeredAddress;

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

				<DetailItem label="Registered address">
					{address
						? `${address.street} ${address.buildingNumber}${
								address.unitNumber ? `/${address.unitNumber}` : ""
							}, ${address.postalCode} ${address.city}, ${address.countryCode}`
						: "—"}
				</DetailItem>

				<DetailItem label="Bank account">{profile?.iban ?? "—"}</DetailItem>
			</dl>

			{company && !company.isProfileComplete ? (
				<>
					<div className="project-inline-warning">
						<AlertTriangle size={15} />

						<span>
							The client profile is incomplete. The project cannot go live, and a contract cannot
							record who signed it and under which address, until this is filled in.
						</span>
					</div>

					<Button
						variant="primary"
						className="mt-3"
						onClick={() => onCompleteProfile(project.companyId)}
					>
						Complete client data
					</Button>
				</>
			) : null}
		</div>
	);
}
