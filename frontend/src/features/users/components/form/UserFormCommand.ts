import type { UserProjection } from "@/api/models";

export interface CreateUserFormCommand {
	create(): void;
}

export interface EditUserFormCommand {
	edit(user: UserProjection): void;
}

export interface ChangeUserRoleFormCommand {
	changeRole(user: UserProjection): void;
}

export interface ChangeUserTeamFormCommand {
	changeTeam(user: UserProjection): void;
}
