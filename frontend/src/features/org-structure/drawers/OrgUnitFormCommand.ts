import type { OrgUnitRow } from "@/api/models";
import type { MoveTarget } from "../types";

/** A person as the chart needs to show them: the projection carries ids, names come from elsewhere. */
export type OrgUnitPerson = {
	userId: string;
	name: string;
	email: string;
};

export interface CreateOrgUnitFormCommand {
	/** `null` creates the top unit, which the domain allows exactly once. */
	create(parent: OrgUnitRow | null): void;
}

export interface RenameOrgUnitFormCommand {
	rename(unit: OrgUnitRow): void;
}

export interface MoveOrgUnitFormCommand {
	move(unit: OrgUnitRow, targets: MoveTarget[]): void;
}

export interface AssignOrgUnitHeadFormCommand {
	assign(unit: OrgUnitRow, candidates: OrgUnitPerson[]): void;
}

export interface AddOrgUnitMemberFormCommand {
	add(unit: OrgUnitRow): void;
}
