import { useRef } from "react";
import type { CompanyContact } from "@/api/models";
import { StandardDataActions } from "@/components/table";
import { DetailOverviewHeader, ItemMark } from "@/components/ui";

import {
	type CompanyContactCommand,
	CompanyContactDelete,
	type CompanyContactDeleteCommand,
} from "@/features/company-contacts/components/form";

import CompanyContactDrawer from "@/features/company-contacts/components/form/CompanyContactDrawer";

interface CompanyContactsProps {
	contacts: CompanyContact[];
	loading?: boolean;
	error?: boolean;
	onRefresh: () => void;
	companyId: string;
}

export function CompanyContacts({
	contacts,
	loading = false,
	error = false,
	companyId,
	onRefresh,
}: CompanyContactsProps) {
	const contactRef = useRef<CompanyContactCommand>(null);
	const deleteContactRef = useRef<CompanyContactDeleteCommand>(null);

	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Contacts"
				description={`${contacts.length} contacts`}
				onAdd={() => {
					contactRef.current?.create(companyId);
				}}
			/>

			{loading && <div className="data-details-loading">Loading contacts...</div>}

			{error && (
				<div className="data-details-state data-details-error">Unable to load contacts.</div>
			)}

			{!loading && !error && contacts.length === 0 && (
				<div className="data-details-state">No contacts added yet.</div>
			)}

			{!loading && !error && contacts.length > 0 && (
				<div className="company-contacts-list">
					{contacts.map((item) => (
						<div key={item.id} className="company-contact">
							<div className="company-contact-main">
								<ItemMark name={item.contact.fullname} />

								<div className="company-contact-info">
									<div className="company-contact-name">
										{item.contact.firstName} {item.contact.lastName}
									</div>

									<div className="company-contact-job">{item.contact.jobTitle || "—"}</div>

									<div className="company-contact-email">
										<a href={`mailto:${item.contact.email}`}>{item.contact.email}</a>
									</div>

									{item.contact.phone && (
										<div className="company-contact-phone">
											<a href={`tel:${item.contact.phone}`}>{item.contact.phone}</a>
										</div>
									)}
								</div>
							</div>

							<StandardDataActions
								onEdit={() => {
									contactRef.current?.editContact(item.id);
								}}
								onDelete={() => {
									deleteContactRef.current?.deleteContact(item.id)
								}}
							/>
						</div>
					))}
				</div>
			)}
			<CompanyContactDrawer
				ref={contactRef}
				onSuccess={() => {
					onRefresh();
				}}
			/>
			<CompanyContactDelete
				ref={deleteContactRef}
				onSuccess={() => {
					onRefresh();
				}}
			/>
		</div>
	);
}
