import { Link } from "@tanstack/react-router";
import { ArrowLeftRight, Building2, ExternalLink } from "lucide-react";
import { useState } from "react";
import { Button, CompanyStatusBadge } from "#/components/ui";
import { DetailItem } from "#/components/ui/details/DataDetails";
import { useGetCompany } from "#/features/companies/pages/hooks";
import { CompanySwitcher } from "./CompanySwitcher";

interface CompanyPanelProps {
	companyId: string;
	onChange: (companyId: string) => void;
}

/** The company the whole screen is about, and the way to put another one there. */
export function CompanyPanel({ companyId, onChange }: CompanyPanelProps) {
	const [picking, setPicking] = useState(false);
	const query = useGetCompany(companyId);
	const company = query.data;
	const address = company?.profile.registeredAddress;

	return (
		<section className="data-details-section workspace-company" aria-label="Selected company">
			<div className="data-details-section-header">
				<div>
					<h2>Selected company</h2>
					<p>Everything on this screen is about it</p>
				</div>
			</div>

			<div className="workspace-company-body">
				{query.isLoading ? <div className="data-details-loading">Loading ...</div> : null}

				{query.isError ? (
					<div className="form-error" role="alert">
						This company could not be loaded.
					</div>
				) : null}

				{company ? (
					<>
						<div className="workspace-company-name">
							<span className="workspace-company-icon">
								<Building2 size={18} />
							</span>
							<div className="min-w-0">
								<h3 className="truncate">{company.name}</h3>
								<CompanyStatusBadge status={company.status} />
							</div>
						</div>

						<dl className="workspace-company-facts">
							<DetailItem label="Tax id">{company.taxId || "—"}</DetailItem>
							<DetailItem label="Location">
								{address ? `${address.city}, ${address.countryCode}` : company.countryCode}
							</DetailItem>
							<DetailItem label="Contact">
								{company.contact ? (
									<>
										<div>
											{company.contact.firstName} {company.contact.lastName}
										</div>
										<div className="data-meta">{company.contact.email}</div>
										<div className="data-meta">{company.contact.phone}</div>
									</>
								) : (
									"—"
								)}
							</DetailItem>
						</dl>

						<Link
							to="/app/companies/$id"
							params={{ id: company.id }}
							search={{ search: undefined }}
							className="workspace-company-link"
						>
							<ExternalLink size={14} /> Company details
						</Link>
					</>
				) : null}

				{picking ? (
					<div className="workspace-company-picker">
						<CompanySwitcher
							onPick={(id) => {
								setPicking(false);
								onChange(id);
							}}
						/>
					</div>
				) : (
					<Button
						variant="secondary"
						icon={<ArrowLeftRight size={14} />}
						onClick={() => setPicking(true)}
					>
						Change company
					</Button>
				)}
			</div>
		</section>
	);
}
