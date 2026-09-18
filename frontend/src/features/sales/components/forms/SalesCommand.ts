import type { OpportunityStage } from "#/api/models";

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

export type OpportunityInfo = {
	id: string;
	title: string;
	stage: OpportunityStage;
	targetStage?: OpportunityStage;
};

export interface ChangeStageRef {
	changeStage: (info: OpportunityInfo) => void;
}

export type FollowUpInfo = {
	followUpActionId: string;
	content: string;
	followDateTime: string;
};

export interface FollowUpRef {
	add: (opportunityId: string) => void;
	edit: (info: FollowUpInfo) => void;
}

export type SalesActionTypes = "edit-opportunity" | "log-activity" | "change-stage";

export interface SalesActionRef {
	onAction: (id: string, action: SalesActionTypes) => void;
	changeStage: (info: OpportunityInfo) => void;
	addFollowUp: (opportunityId: string) => void;
	editFollowUp: (info: FollowUpInfo) => void;
}
