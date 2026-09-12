import { Users } from "lucide-react";

import { ActionMenu } from "@/components/ui/ActionMenu";

interface OrganizationsActionsProps {
	onAddUser: () => void;
}
export function OrganizationsActions({ onAddUser }: OrganizationsActionsProps) {
	return (
		<div className="table-actions">
			<ActionMenu actions={[{ label: "Add user", icon: Users, action: onAddUser }]} />
		</div>
	);
}
