export interface PostToChannelCommand {
	postToChannel: (id: string) => void;
}

export interface ChangeJobPostStatusCommand {
	changeStatus: (id: string) => void;
}
