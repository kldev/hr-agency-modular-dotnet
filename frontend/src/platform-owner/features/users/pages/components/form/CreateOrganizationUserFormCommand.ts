export interface CreateOrganizationUserFormCommand {
	create(organizationId: string, organizationName: string): void;
}
