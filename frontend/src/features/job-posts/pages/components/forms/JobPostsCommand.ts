export interface PostToChannelCommand {
	postToChannel: (id: string) => void;
}

export interface ChangeJobPostStatusCommand {
	changeStatus: (id: string) => void;
}

export interface ChangeJobPostRecruiterCommand {
	/** `recruiterId` seeds the picker with whoever is responsible today. */
	changeRecruiter: (id: string, recruiterId: string) => void;
}
