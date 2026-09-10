import { MessageSquare } from "lucide-react";
import type React from "react";
import { useState } from "react";
import { Page } from "@/components/layout";
import { EmptyState, WorkInProgress } from "@/components/ui";

const InterviewsPage: React.FC = () => {
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
			title="Interviews"
			description="Schedule and manage interviews with job applicants."
			onRefresh={handleOnRefresh}
			loading={loading}
			page={0}
			isEmpty={true}
			emptyState={
				<EmptyState title="No interviews found">
					<MessageSquare size={24} />
				</EmptyState>
			}
		>
			<WorkInProgress />
		</Page>
	);
};

export default InterviewsPage;
