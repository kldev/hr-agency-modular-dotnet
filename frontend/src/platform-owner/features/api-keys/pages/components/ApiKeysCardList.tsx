import type { ServiceApiKeyRow } from "#/api/models";
import { Button } from "@/components/ui";
import { DetailItem } from "@/components/ui/details/DataDetails";
import { formatDateTime } from "@/utlis/dateUtils";
import { KeyPrefix, KeyState } from "./ApiKeysTable";

interface Props {
	keys: ServiceApiKeyRow[];
	onRevoke: (key: ServiceApiKeyRow) => void;
}

export function ApiKeysCardList({ keys, onRevoke }: Props) {
	return (
		<div className="data-mobile-view">
			{keys.map((key) => (
				<div
					key={key.id}
					className="flex flex-col gap-3 pb-5 border mb-5 mt-5 shadow-subtle card-border"
				>
					<dl className="data-details-list">
						<DetailItem label="Name">{key.name}</DetailItem>
						<DetailItem label="Key">
							<KeyPrefix apiKey={key} />
						</DetailItem>
						<DetailItem label="Issued">{formatDateTime(key.createdAt)}</DetailItem>
						<DetailItem label="State">
							<KeyState apiKey={key} />
						</DetailItem>
					</dl>

					{key.revokedAt ? null : (
						<div className="px-4">
							<Button variant="danger" onClick={() => onRevoke(key)}>
								Revoke
							</Button>
						</div>
					)}
				</div>
			))}
		</div>
	);
}
