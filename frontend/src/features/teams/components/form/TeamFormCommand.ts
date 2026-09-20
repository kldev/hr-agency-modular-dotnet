export interface CreateTeamFormCommand {
	create(): void;
}

export interface RenameTeamFormCommand {
	rename(teamId: string, currentName: string): void;
}
