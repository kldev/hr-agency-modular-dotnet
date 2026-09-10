import type { CompanyProjection } from "@/api/models";

interface CompaniesTableProps {
	companies: CompanyProjection[];
	onEdit?: (company: CompanyProjection) => void;
	onDelete?: (company: CompanyProjection) => void;
}

function CompanyMark({ company }: { company: CompanyProjection }) {
	const initials = company.name
		.split(" ")
		.slice(0, 2)
		.map((part) => part[0])
		.join("")
		.toUpperCase();

	return <span className="company-avatar">{initials}</span>;
}

export function CompaniesTable({ companies }: CompaniesTableProps) {
	return (
		<div className="table-container">
			<table className="table">
				<thead>
					<tr>
						<th scope="col">Company</th>
						<th scope="col">Industry</th>
						<th scope="col">Tax</th>
						<th scope="col" className="table-number">
							Acitve job posts
						</th>
						<th scope="col" className="table-number">
							Applicants count
						</th>
						<th scope="col">Updated</th>
						<th scope="col" aria-label="Actions" />
					</tr>
				</thead>

				<tbody>
					{companies.map((company) => (
						<tr key={company.id}>
							<td>
								<div className="table-cell-content">
									<CompanyMark company={company} />
									<div>
										<div className="company-name">{company.name}</div>
										<div className="company-meta">
											{company.countryCode} · {company.website}
										</div>
									</div>
								</div>
							</td>
							<td>{company.industry}</td>
							<td>{company.taxId}</td>
							<td className="table-number">{company.activeJobsPostCount}</td>
							<td className="table-number">{company.applicantsCount}</td>
							<td>{company.modifiedAt}</td>
							<td></td>
						</tr>
					))}
				</tbody>
			</table>
		</div>
	);
}
