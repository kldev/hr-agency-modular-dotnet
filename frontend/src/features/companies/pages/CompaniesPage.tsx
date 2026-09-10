import { Building2, ConstructionIcon } from "lucide-react";
import type React from "react";
import { useState } from "react";
import { getApiCompanies } from "@/api/endpoints";
import type { SliceResponseOfCompanyProjection } from "@/api/models";
import { Page } from "@/components/layout";
import { EmptyState } from "@/components/ui";
import { UsersPicker } from "@/components/ui/pickers/UsersPicker";
import { CompaniesTable } from "./components/CompaniesTable";

const CompaniesPage: React.FC = () => {
	const [loading, setLoading] = useState(false);
	const [companies, setCompanies] = useState<SliceResponseOfCompanyProjection>();
	const handleOnRefresh = async () => {
		setLoading(true);

		const result = await getApiCompanies({ pageSize: 20, page: 0 });

		setCompanies(result);
		setLoading(false);
		return Promise.resolve();
	};

	const [recruiterId, setRecruiterId] = useState<string | null>(
		null,
	);

	const [recruiterInput, setRecruiterInput] =
		useState("");

	return (
		<Page
			title="Companies"
			description="Manage companies and their recruitment relationships."
			onRefresh={handleOnRefresh}
			loading={loading}
			page={0}
			isEmpty={companies !== undefined && companies?.content.length === 0}
			emptyState={
				<EmptyState title="No companies found">
					<Building2 size={24} />
				</EmptyState>
			}
		>
			<div className="space-y-5 p-5 max-w-120">
				<UsersPicker value={recruiterId}
					inputValue={recruiterInput}
					onChange={(id) => {
						setRecruiterId(id);
					}}
					onInputChange={setRecruiterInput}
					allowCustomValue={false} />
			</div>
			<CompaniesTable companies={companies?.content ?? []} />
		</Page>
	);
};

export default CompaniesPage;
