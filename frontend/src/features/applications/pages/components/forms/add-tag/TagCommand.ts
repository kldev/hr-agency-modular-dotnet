import type { Tag } from "#/api/models";

export type TagTarget = "application" | "candidate";

export interface AddTagCommand {
	addTag: (id: string, display: string, target: TagTarget) => void;
}

export interface TagCommand {
	addTag: (id: string, display: string, target: TagTarget) => void;
	editTags: (id: string, tags: Tag[], target: TagTarget) => void;
}
