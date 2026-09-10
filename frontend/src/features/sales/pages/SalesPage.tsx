import { DollarSign } from "lucide-react";
import type React from "react";
import { useState } from "react";
import { Page } from "@/components/layout";
import { EmptyState, WorkInProgress } from "@/components/ui";
import { OpportunityStageBadge } from "@/components/ui/Badge";

const SalesPage: React.FC = () => {
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
			title="Sales"
			description="Manage your leads and sales opportunities THROUGH the pipeline."
			onRefresh={handleOnRefresh}
			loading={loading}
			page={0}
			isEmpty={true}
			emptyState={
				<EmptyState title="No sales opportunities found">
					<DollarSign size={24} />
				</EmptyState>
			}
		>
			<WorkInProgress />
			<div className="flex gap-5 flex-row">
				<OpportunityStageBadge status={"New"}></OpportunityStageBadge>
				<OpportunityStageBadge status={"Viewed"}></OpportunityStageBadge>
				<OpportunityStageBadge status={"Contacted"}></OpportunityStageBadge>
				<OpportunityStageBadge status={"Proposal"}></OpportunityStageBadge>
				<OpportunityStageBadge status={"Qualified"}></OpportunityStageBadge>
				<OpportunityStageBadge status={"Won"}></OpportunityStageBadge>
				<OpportunityStageBadge status={"Lost"}></OpportunityStageBadge>
			</div>
		</Page>
	);
};

export default SalesPage;
