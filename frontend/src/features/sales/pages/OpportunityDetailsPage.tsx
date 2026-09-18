import { DataDetails, DetailsHeader, DetailsLoading } from "@/components/ui";
import { DataDetailsLayout } from "@/components/ui/details/DataDetails";
import { SalesStageBadge } from "../components";
import { useGetOpportunity } from "../hooks";

const OpportunityDetailsPage: React.FC<{ id: string }> = ({ id }) => {
	const query = useGetOpportunity(id);

	if (!id || query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !query.data} />
		);
	}

	const opportunity = query.data;

	return (
		<DataDetails>
			<DetailsHeader
				name={opportunity.title}
				detailsAddons={
					<div className="data-details-header-meta">
						<SalesStageBadge stage={opportunity.stage} />
					</div>
				}
			/>

			<DataDetailsLayout main={null} sidebar={null} />
		</DataDetails>
	);
};

export default OpportunityDetailsPage;
