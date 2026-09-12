export interface CreateOrganizationCommand {
	create(): void;
}

export interface EditOrganizationCommand {
	edit(id: string): void;
}
