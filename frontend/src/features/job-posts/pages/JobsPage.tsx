import { BriefcaseBusiness } from "lucide-react";
import type React from "react";
import { useState } from "react";
import { Page } from "@/components/layout";
import { EmptyState, WorkInProgress } from "@/components/ui";

const AplicationsPage: React.FC = () => {
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
			title="Job postings"
			description=" Manage job offers, candidates and recruitment processes."
			onRefresh={handleOnRefresh}
			loading={loading}
			page={0}
			isEmpty={true}
			emptyState={
				<EmptyState title="No job posts found">
					<BriefcaseBusiness />
				</EmptyState>
			}
		>
			<WorkInProgress />
		</Page>
	);
};

export default AplicationsPage;
