import { KeyRound, Plus } from "lucide-react";
import { useRef, useState } from "react";
import { toast } from "sonner";
import type { ServiceApiKeyRow } from "#/api/models";
import { Page } from "@/components/layout";
import { Button, ConfirmDialog, EmptyState } from "@/components/ui";
import {
	ApiKeysCardList,
	ApiKeysTable,
	type IssueApiKeyCommand,
	IssueApiKeyDrawer,
} from "./components";
import { useRevokeServiceApiKey, useServiceApiKeys } from "./hooks";

/**
 * Keys for programs - today the public job board. The owner issues them because they stand above
 * every organization, the same way the board does.
 */
export function ApiKeysPage() {
	const issueRef = useRef<IssueApiKeyCommand>(null);
	const [toRevoke, setToRevoke] = useState<ServiceApiKeyRow | null>(null);

	const query = useServiceApiKeys();
	const revoke = useRevokeServiceApiKey();

	const keys = query.data ?? [];

	return (
		<>
			<Page
				className="has-mobile-view"
				title="API keys"
				description="Keys for programs rather than people. The public job board presents one to read offers and file applications."
				onRefresh={() => query.refetch()}
				loading={query.isPending}
				isEmpty={query.isFetched && keys.length === 0}
				emptyState={
					<EmptyState
						title="No keys yet"
						description="Issue one for the public job board - without it the board cannot reach the API."
					>
						<KeyRound size={24} />
					</EmptyState>
				}
				headerAddon={
					<Button
						variant="primary"
						icon={<Plus size={15} />}
						onClick={() => issueRef.current?.issue()}
					>
						Issue key
					</Button>
				}
			>
				<ApiKeysTable keys={keys} onRevoke={setToRevoke} />
				<ApiKeysCardList keys={keys} onRevoke={setToRevoke} />
			</Page>

			<IssueApiKeyDrawer ref={issueRef} />

			<ConfirmDialog
				open={Boolean(toRevoke)}
				title="Revoke this key?"
				description={
					toRevoke
						? `${toRevoke.name} (${toRevoke.displayPrefix}…) stops working at once. Whatever uses it - the job board, most likely - is refused until it gets a new key. This cannot be undone.`
						: ""
				}
				confirmLabel="Revoke"
				loading={revoke.isPending}
				onConfirm={() =>
					toRevoke &&
					revoke.mutate(toRevoke.id, {
						onSuccess: () => setToRevoke(null),
						onError: () => toast.error("The key could not be revoked"),
					})
				}
				onClose={() => setToRevoke(null)}
			/>
		</>
	);
}
