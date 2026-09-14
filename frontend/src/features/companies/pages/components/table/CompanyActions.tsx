import { useNavigate } from "@tanstack/react-router";
import { Pencil, PersonStanding, Settings2 } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface CompanyActionsProps {
	id: string;
	onEdit: () => void;
	onAddContact: () => void;
}

export function CompanyActions({ onEdit, onAddContact, id }: CompanyActionsProps) {
	const navigate = useNavigate();

	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{ label: "Edit", icon: Pencil, action: onEdit },
					{ label: "Add contact", icon: PersonStanding, action: onAddContact },
					{
						label: "Open details",
						icon: Settings2,
						action: () => {
							navigate({ to: "/app/companies/$id", params: { id } });
						},
					},
				]}
			/>
		</div>
	);
}
