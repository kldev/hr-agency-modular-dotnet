import { ItemMark } from "@/components/ui";
import { useGetCompanyContacts } from "@/features/companies/pages/hooks";

interface OpportunityContactsProps {
	companyId: string;
}

export function OpportunityContacts({ companyId }: OpportunityContactsProps) {
	const query = useGetCompanyContacts(companyId);

	const contacts = query.data ?? [];

	return (
		<section className="data-details-section">
			<div className="data-details-section-header">
				<div>
					<h2>Contact</h2>
					<p>{`${contacts.length} contacts of the company`}</p>
				</div>
			</div>

			{query.isLoading ? <div className="data-details-loading">Loading contacts...</div> : null}

			{query.isError ? <div className="data-details-error">Unable to load contacts.</div> : null}

			{!query.isLoading && !query.isError && contacts.length === 0 ? (
				<div className="data-details-empty">No contacts added yet.</div>
			) : null}

			{contacts.length ? (
				<div className="sales-contact-list">
					{contacts.map((item) => (
						<div key={item.id} className="sales-contact">
							<ItemMark name={item.contact.fullname} />

							<div className="sales-contact-info">
								<div className="sales-contact-name">
									{item.contact.firstName} {item.contact.lastName}
								</div>

								<div className="sales-contact-job">{item.contact.jobTitle || "—"}</div>

								<a href={`mailto:${item.contact.email}`}>{item.contact.email}</a>

								{item.contact.phone ? (
									<a href={`tel:${item.contact.phone}`}>{item.contact.phone}</a>
								) : null}
							</div>
						</div>
					))}
				</div>
			) : null}
		</section>
	);
}
