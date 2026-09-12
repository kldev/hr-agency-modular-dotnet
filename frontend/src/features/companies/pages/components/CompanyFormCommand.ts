export interface CreateCompanyFormCommand {
	create: () => void;
}

export interface EditCompanyFormCommand {
	edit: (id: string) => void;
}
