import type { WorkerDocument, WorkerProjection } from "@/api/models";

export interface ChangeWorkerStatusFormCommand {
	changeStatus: (worker: WorkerProjection) => void;
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
