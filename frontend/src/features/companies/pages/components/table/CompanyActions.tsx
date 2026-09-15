import { useNavigate } from "@tanstack/react-router";
import { CircleDollarSign, Pencil, PersonStanding, Settings2 } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface CompanyActionsProps {
	id: string;
	onEdit: () => void;
	onAddContact: () => void;
	onAddOpportunity: () => void;
	mode?: "table" | "details";
}

export function CompanyActions({
	onEdit,
	onAddContact,
	onAddOpportunity,
	id,
	mode = "table",
}: CompanyActionsProps) {
	const navigate = useNavigate();

	if (mode === "details") {
		return (
			<div className="table-actions">
				<ActionMenu
					actions={[{ label: "Add opportunity", icon: CircleDollarSign, action: onAddOpportunity }]}
				/>
			</div>
		);
	}

	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{ label: "Edit", icon: Pencil, action: onEdit },
					{ label: "Add contact", icon: PersonStanding, action: onAddContact },
					{ label: "Add opportunity", icon: CircleDollarSign, action: onAddOpportunity },
					{
						label: "Open details",
						icon: Settings2,
						action: () => {
							navigate({ to: "/app/companies/$id", params: { id: id }, search: { search: "" } });
						},
					},
				]}
			/>
		</div>
	);
}
