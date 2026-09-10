import { Globe2 } from "lucide-react";
import type React from "react";
import { useState } from "react";
import { Page } from "@/components/layout";
import { EmptyState, WorkInProgress } from "@/components/ui";

const OrganizationsPage: React.FC = () => {
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
			title="Organizations"
			description=" Recruitment workspaces and their configuration"
			onRefresh={handleOnRefresh}
			loading={loading}
			page={0}
			isEmpty={true}
			emptyState={
				<EmptyState title="No organizations found">
					<Globe2 size={24} />
				</EmptyState>
			}
		>
			<WorkInProgress />
		</Page>
	);
};

export default OrganizationsPage;
