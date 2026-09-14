import { useNavigate } from "@tanstack/react-router";
import { BriefcaseBusiness, Users } from "lucide-react";
import type React from "react";
import { Page } from "@/components/layout";
import { Button, WorkInProgress } from "@/components/ui";
import { DashboardMetrics } from "../components/ DashboardMetrics";

const DashboardPage: React.FC = () => {
	const navigate = useNavigate();

	return (
		<Page
			title="Dashboard"
			description="Overview of your recruitment activity and upcoming tasks."
			isEmpty={true}
			emptyState={<div></div>}
			headerAddon={
				<div className="flex gap-2">
					<Button variant="secondary" onClick={() => navigate({ to: "/app/applications" })}>
						<Users className="size-4" />
						Job applications
					</Button>

					<Button onClick={() => navigate({ to: "/app/job-descriptions" })}>
						<BriefcaseBusiness className="size-4" />
						New job posting
					</Button>
				</div>
			}
		>
			<DashboardMetrics />

			<WorkInProgress />
		</Page>
	);
};

export default DashboardPage;
