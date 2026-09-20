import { useNavigate } from "@tanstack/react-router";
import { Ban, Pencil, Settings2 } from "lucide-react";
import type { ActionMenuItem } from "@/components/ui/ActionMenu";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface LegalEntityActionsProps {
	id: string;
	name: string;
	isClosed: boolean;
	onEdit: () => void;
	onClose: () => void;
	mode?: "table" | "details";
}

export function LegalEntityActions({
	id,
	name,
	isClosed,
	onEdit,
	onClose,
	mode = "table",
}: LegalEntityActionsProps) {
	const navigate = useNavigate();

	const actions: ActionMenuItem[] = [{ label: "Edit", icon: Pencil, action: onEdit }];

	// Closing twice is refused by the domain, so it is not offered twice either.
	if (!isClosed) {
		actions.push({ label: "Close", icon: Ban, action: onClose });
	}

	if (mode === "table") {
		actions.push({
			label: "Open details",
			icon: Settings2,
			action: () => {
				navigate({ to: "/app/legal-entities/$id", params: { id } });
			},
		});
	}

	return (
		<div className="table-actions">
			<ActionMenu actions={actions} ariaLabel={`Actions for ${name}`} />
		</div>
	);
}
