export interface CreateCandidateFormCommand {
	create(): void;
}

export interface EditCandidateFormCommand {
	edit(candidateId: string): void;
}
