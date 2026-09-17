export interface ChangeStatusCommand {
	changeStatus: (id: string) => void;
}

export interface ChangeRecruiterCommand {
	/** `recruiterId` seeds the picker with whoever is responsible today. */
	changeRecruiter: (id: string, recruiterId: string) => void;
}
