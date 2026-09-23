import type { WorkerDocument, WorkerProjection, WorkerStatus } from "@/api/models";

export interface ChangeWorkerStatusFormCommand {
	/** `target` preselects the new status - the column a card was dropped on. */
	changeStatus: (worker: WorkerProjection, target?: WorkerStatus) => void;
}

export interface RecordWorkAuthorisationFormCommand {
	record: (worker: WorkerProjection) => void;
}

export interface AttachWorkerDocumentFormCommand {
	attach: (workerId: string) => void;
}

export interface EditWorkerDocumentFormCommand {
	edit: (workerId: string, document: WorkerDocument) => void;
}
