import type { WorkerContractType } from "@/api/models";

/**
 * What we sign with a person. Shared vocabulary, in its own folder for the reason the backend moved
 * `WorkerContractType` into `SharedKernel`: positions ask it about somebody we send to a client and
 * the agency register asks it about somebody who works for us, and neither owns the other's answer.
 */
export const workerContractTypes: Record<WorkerContractType, string> = {
	EmploymentContract: "Employment contract",
	TemporaryEmploymentContract: "Temporary employment contract",
	MandateContract: "Mandate contract",
	SelfEmployed: "Self-employed (B2B)",
	Other: "Other",
};

/**
 * Mirrors `Agency/Domain/Employment/TimeRecordPolicy.cs`. It is law, not configuration: a mandate
 * has carried the duty to record hours since the hourly minimum came in, and a contract for
 * specific work settles a result rather than time.
 */
export const contractRequiresTimeRecord: Record<WorkerContractType, boolean> = {
	EmploymentContract: true,
	TemporaryEmploymentContract: true,
	MandateContract: true,
	SelfEmployed: false,
	Other: false,
};
