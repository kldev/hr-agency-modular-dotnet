export interface CompanyContactCommand {
	create: (companyId: string) => void;
	editContact: (contactId: string) => void;
}

export interface CompanyDeleteCommand {
	deleteContact: (contactId: string) => void;
}
