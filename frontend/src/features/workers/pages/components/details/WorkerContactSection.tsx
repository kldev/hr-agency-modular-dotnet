import type { WorkerProjection } from "@/api/models";
import { DetailItem, DetailOverviewHeader } from "@/components/ui";
import { formatAddress } from "@/utlis/formatRecord";

interface WorkerContactSectionProps {
	worker: WorkerProjection;
}

export function WorkerContactSection({ worker }: WorkerContactSectionProps) {
	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Contact"
				description="How to reach them, and where they live. The address is optional and often missing."
			/>

			<dl className="data-details-list">
				<DetailItem label="Email">
					{worker.email ? <a href={`mailto:${worker.email}`}>{worker.email}</a> : "—"}
				</DetailItem>

				<DetailItem label="Phone">
					{worker.phoneNumber ? (
						<a href={`tel:${worker.phoneNumber}`}>{worker.phoneNumber}</a>
					) : (
						"—"
					)}
				</DetailItem>

				<DetailItem label="Address">
					{worker.address ? formatAddress(worker.address) : "—"}
				</DetailItem>

				<DetailItem label="Note">{worker.note || "—"}</DetailItem>
			</dl>
		</div>
	);
}
