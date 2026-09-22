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

export interface SetUserAvatarFormCommand {
	setAvatar(user: UserProjection): void;
}

/**
 * Takes the bare facts rather than a `UserProjection`, because the org chart drives this too and it
 * only knows a member's id and name.
 */
export interface ImpersonateUserFormCommand {
	impersonate(user: { id: string; fullName?: string | null }): void;
}
