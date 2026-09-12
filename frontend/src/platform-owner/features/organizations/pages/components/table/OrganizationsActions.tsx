import { Pencil, Users } from "lucide-react";

import { ActionMenu } from "@/components/ui/ActionMenu";

interface OrganizationsActionsProps {
	onAddUser: () => void;
	onEdit: () => void;
}
export function OrganizationsActions({ onAddUser, onEdit }: OrganizationsActionsProps) {
	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{ label: "Edit", icon: Pencil, action: onEdit },
					{ label: "Add user", icon: Users, action: onAddUser },
				]}
			/>
		</div>
	);
}
