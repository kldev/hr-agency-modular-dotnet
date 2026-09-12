export interface CompanyContactCommand {
	create: (companyId: string) => void;
	editContact: (contactId: string) => void;
}
