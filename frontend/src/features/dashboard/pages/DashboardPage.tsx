import type React from "react";
import { Page } from "@/components/layout";
import { WorkInProgress } from "@/components/ui";

const DashboardPage: React.FC = () => {
	return (
		<Page
			title="Dashboard"
			description="Overview of your recruitment activity and upcoming tasks."
			isEmpty={true}
			emptyState={<div></div>}
		>
			<WorkInProgress />
		</Page>
	);
};

export default DashboardPage;
