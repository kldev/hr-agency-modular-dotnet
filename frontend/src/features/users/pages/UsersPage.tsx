import { Users } from "lucide-react";
import type React from "react";
import { useState } from "react";
import { Page } from "@/components/layout";
import { EmptyState, WorkInProgress } from "@/components/ui";

const UsersPage: React.FC = () => {
	const [loading, setLoading] = useState(false);
	const handleOnRefresh = async () => {
		setLoading(true);

		window.setTimeout(() => {
			setLoading(false);
		}, 500);

		return Promise.resolve();
	};
	return (
		<Page
			title="Users"
			description="People with access to the organization"
			onRefresh={handleOnRefresh}
			loading={loading}
			page={0}
			isEmpty={true}
			emptyState={
				<EmptyState title="No users found">
					<Users size={24} />
				</EmptyState>
			}
		>
			<WorkInProgress />
		</Page>
	);
};

export default UsersPage;
