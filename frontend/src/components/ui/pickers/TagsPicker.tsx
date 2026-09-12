import { Building2 } from "lucide-react";
import { getTagsSuggestions } from "@/api/endpoints";
import type { Tag } from "@/api/models";
import { SuggestionPicker, type SuggestionPickerProps } from "./SuggestionPicker";

export type TagsPickerProps = Omit<
	SuggestionPickerProps<Tag>,
	"loadSuggestions" | "getKey" | "getLabel"
>;

type Props = TagsPickerProps;

export function TagsPicker(props: Props) {
	const searchCompanies = async (
		query: string,
		signal: AbortSignal,
	): Promise<Tag[]> => {
		return await getTagsSuggestions({ search: query ?? "" }, undefined, signal);
	};

	return (
		<SuggestionPicker<Tag>
			{...props}
			placeholder={props.placeholder ?? "Search tags.."}
			loadSuggestions={searchCompanies}
			getKey={(item) => item.id}
			getLabel={(item) => item.name}
			getDescription={(item) => `${item.category}`}
			renderItem={(item, selected) => (
				<div className="suggestion-picker-default-item">
					<div className="suggestion-picker-avatar">
						<Building2 size={16} />
					</div>

					<div className="suggestion-picker-item-content">
						<div className="suggestion-picker-item-label">{item.name}</div>

						<div className="suggestion-picker-company-meta">
							<span>`${item.category}`</span>
						</div>
					</div>

					{selected && <span className="suggestion-picker-selected-mark">✓</span>}
				</div>
			)}
		/>
	);
}
