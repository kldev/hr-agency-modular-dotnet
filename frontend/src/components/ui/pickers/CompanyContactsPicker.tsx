import { Building2 } from "lucide-react";
import { getCompanyContactsSuggestions } from "@/api/endpoints";
import type { CompanyContact } from "@/api/models";
import { SuggestionPicker, type SuggestionPickerProps } from "./SuggestionPicker";

export type CompanyContactsPickerProps = Omit<
	SuggestionPickerProps<CompanyContact>,
	"loadSuggestions" | "getKey" | "getLabel"
>;

type OwnProps = {
	companyId: string;
};

type Props = CompanyContactsPickerProps & OwnProps;

export function CompanyContactsPicker(props: Props) {
	const searchCompanies = async (query: string, signal: AbortSignal): Promise<CompanyContact[]> => {
		return await getCompanyContactsSuggestions(
			{ search: query ?? "", companyId: props.companyId },
			undefined,
			signal,
		);
	};

	return (
		<SuggestionPicker<CompanyContact>
			{...props}
			placeholder={props.placeholder ?? "Search company contacts..."}
			loadSuggestions={searchCompanies}
			getKey={(item) => item.id}
			getLabel={(item) => item.contact.fullname ?? item.contact.email}
			getDescription={(item) => `${item.companyName}`}
			renderItem={(item, selected) => (
				<div className="suggestion-picker-default-item">
					<div className="suggestion-picker-avatar">
						<Building2 size={16} />
					</div>

					<div className="suggestion-picker-item-content">
						<div className="suggestion-picker-item-label">
							{item.contact.fullname ?? item.contact.email}
						</div>

						<div className="suggestion-picker-company-meta">
							<span>`${item.contact.email}`</span>
						</div>
						<div className="suggestion-picker-company-meta">
							<span>`${item.companyName}`</span>
						</div>
					</div>

					{selected && <span className="suggestion-picker-selected-mark">✓</span>}
				</div>
			)}
		/>
	);
}
