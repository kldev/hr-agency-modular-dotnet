export interface CreateOpportunityRef {
	create: (companyId: string, companyName?: string) => void;
}

export interface EditOpportunityRef {
	edit: (opportunityId: string) => void;
}

export interface LogActionRef {
	create: (companyId: string, companyName?: string) => void;
}

export interface SalesRef extends EditOpportunityRef, LogActionRef {}
