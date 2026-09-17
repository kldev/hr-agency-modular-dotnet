import { useQueryClient } from "@tanstack/react-query";
import { Building2 } from "lucide-react";
import { useCallback } from "react";
import { getCompaniesSuggestions } from "@/api/endpoints";
import type { CompanySuggestion } from "@/api/models";
import { suggestionKeys } from "@/api/query-keys";
import { useCompanySuggestion } from "@/hooks";
import { SuggestionPicker, type SuggestionPickerProps } from "./SuggestionPicker";

export type CompaniesPickerProps = Omit<
	SuggestionPickerProps<CompanySuggestion>,
	"loadSuggestions" | "selectedItem" | "getKey" | "getLabel"
>;

type Props = CompaniesPickerProps;

const getKey = (item: CompanySuggestion) => item.id;
const getLabel = (item: CompanySuggestion) => item.name;
const getDescription = (item: CompanySuggestion) => `${item.taxNumber}, ${item.countryCode}`;

export function CompaniesPicker({ onChange, ...props }: Props) {
	const queryClient = useQueryClient();

	const searchCompanies = useCallback(
		async (query: string, signal: AbortSignal): Promise<CompanySuggestion[]> => {
			return await getCompaniesSuggestions({ search: query ?? "" }, undefined, signal);
		},
		[],
	);

	/*
	 * Resolves `value` back into a company whenever the picker is mounted without a label - a wizard
	 * step the user navigated back to, or an edit form seeded from a record.
	 */
	const { data: selectedCompany } = useCompanySuggestion(props.value ?? "");

	/*
	 * Seeding the cache with the item the dropdown just handed us keeps the query above from firing
	 * a second request for a company we already have.
	 */
	const handleChange = useCallback(
		(value: string | null, item?: CompanySuggestion) => {
			if (item) {
				queryClient.setQueryData(suggestionKeys.company(item.id), item);
			}

			onChange(value, item);
		},
		[onChange, queryClient],
	);

	return (
		<SuggestionPicker<CompanySuggestion>
			{...props}
			placeholder={props.placeholder ?? "Search companies..."}
			allowCustomValue={props.allowCustomValue ?? false}
			onChange={handleChange}
			loadSuggestions={searchCompanies}
			selectedItem={selectedCompany ?? null}
			getKey={getKey}
			getLabel={getLabel}
			getDescription={getDescription}
			renderItem={(item, selected) => (
				<div className="suggestion-picker-default-item">
					<div className="suggestion-picker-avatar">
						<Building2 size={16} />
					</div>

					<div className="suggestion-picker-item-content">
						<div className="suggestion-picker-item-label">{item.name}</div>

						<div className="suggestion-picker-company-meta">
							<span>
								{item.taxNumber}, {item.countryCode}
							</span>
						</div>
					</div>

					{selected && <span className="suggestion-picker-selected-mark">✓</span>}
				</div>
			)}
		/>
	);
}
