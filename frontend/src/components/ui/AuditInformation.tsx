import type { UserSnapshot } from "#/api/models";
import { formatDateTime } from "#/utlis";
import { DetailItem } from "./details";

type AuditInformationProps = {
	createdBy: UserSnapshot;
	createdAt: string;
	modifiedBy: UserSnapshot | undefined | null;
	modifiedAt: string | undefined | null;
};

export function AuditInformation({
	createdAt,
	createdBy,
	modifiedAt,
	modifiedBy,
}: AuditInformationProps) {
	return (
		<section className="data-details-section">
			<div className="data-details-section-header">
				<div>
					<h2>Audit</h2>
					<p>Record information</p>
				</div>
			</div>

			<dl className="data-details-list">
				<DetailItem label="Created">{formatDateTime(createdAt)}</DetailItem>

				<DetailItem label="Created by">{createdBy?.fullname}</DetailItem>

				<DetailItem label="Updated">{formatDateTime(modifiedAt)}</DetailItem>

				<DetailItem label="Modified by">{modifiedBy?.fullname || "-"}</DetailItem>
			</dl>
		</section>
	);
}
