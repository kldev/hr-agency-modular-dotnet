export type InitialCompanyOptions = {
	companyId: string;
	companyName?: string;
};

export interface CreateOpportunityRef {
	create: (intial?: InitialCompanyOptions) => void;
}

export interface EditOpportunityRef {
	edit: (opportunityId: string) => void;
}

export interface LogActionRef {
	log: (opportunityId: string) => void;
}

export type SalesActionTypes = "edit-opportunity" | "log-activity";

export interface SalesActionRef {
	onAction: (id: string, action: SalesActionTypes) => void;
}
