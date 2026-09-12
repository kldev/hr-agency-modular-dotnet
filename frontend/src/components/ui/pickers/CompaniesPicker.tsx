import { Building2 } from "lucide-react";
import { getCompaniesSuggestions } from "@/api/endpoints";
import type { CompanySuggestion } from "@/api/models";
import { SuggestionPicker, type SuggestionPickerProps } from "./SuggestionPicker";

export type CompaniesPickerProps = Omit<
	SuggestionPickerProps<CompanySuggestion>,
	"loadSuggestions" | "getKey" | "getLabel"
>;

type Props = CompaniesPickerProps;

export function CompaniesPicker(props: Props) {
	const searchCompanies = async (
		query: string,
		signal: AbortSignal,
	): Promise<CompanySuggestion[]> => {
		return await getCompaniesSuggestions({ search: query ?? "" }, undefined, signal);
	};

	return (
		<SuggestionPicker<CompanySuggestion>
			{...props}
			placeholder={props.placeholder ?? "Search companies..."}
			loadSuggestions={searchCompanies}
			getKey={(item) => item.id}
			getLabel={(item) => item.name}
			getDescription={(item) => `${item.taxNumber}, ${item.countryCode}`}
			renderItem={(item, selected) => (
				<div className="suggestion-picker-default-item">
					<div className="suggestion-picker-avatar">
						<Building2 size={16} />
					</div>

					<div className="suggestion-picker-item-content">
						<div className="suggestion-picker-item-label">{item.name}</div>

						<div className="suggestion-picker-company-meta">
							<span>`${item.taxNumber}, ${item.countryCode}`</span>
						</div>
					</div>

					{selected && <span className="suggestion-picker-selected-mark">✓</span>}
				</div>
			)}
		/>
	);
}
