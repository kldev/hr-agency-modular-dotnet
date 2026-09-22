import type { AgencyEmploymentProjection } from "@/api/models";

/**
 * The commands the register and the user card both drive. Starting an engagement may be handed the
 * person it is for: on somebody's own page there is nothing to pick.
 */
export interface StartEmploymentFormCommand {
	start: (userId?: string) => void;
}

export interface ChangeEmploymentTermsFormCommand {
	changeTerms: (employment: AgencyEmploymentProjection) => void;
}

export interface EndEmploymentFormCommand {
	end: (employment: AgencyEmploymentProjection) => void;
}
