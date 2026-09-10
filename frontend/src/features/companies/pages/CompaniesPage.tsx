import { Building2, ConstructionIcon } from "lucide-react";
import type React from "react";
import { useState } from "react";
import { getApiCompanies } from "@/api/endpoints";
import type { SliceResponseOfCompanyProjection } from "@/api/models";
import { Page } from "@/components/layout";
import { EmptyState } from "@/components/ui";

const CompaniesPage: React.FC = () => {
	const [loading, setLoading] = useState(false);
	const [companies, setCompanies] = useState<SliceResponseOfCompanyProjection>();
	const handleOnRefresh = async () => {
		setLoading(true);

		const result = await getApiCompanies({ pageSize: 100, page: 0 });

		setCompanies(result);
		setLoading(false);
		return Promise.resolve();
	};
	return (
		<Page
			title="Companies"
			description="Manage companies and their recruitment relationships."
			onRefresh={handleOnRefresh}
			loading={loading}
			page={0}
			isEmpty={true}
			emptyState={
				<EmptyState title="No companies found">
					<Building2 size={24} />
				</EmptyState>
			}
		>
			<div className="flex items-center flex-col align-middle text-orange-800 text-4xl">
				<ConstructionIcon />
				<h1>Work in progress</h1>
			</div>
			<div>
				<code>{JSON.stringify(companies)}</code>
			</div>
		</Page>
	);
};

export default CompaniesPage;
