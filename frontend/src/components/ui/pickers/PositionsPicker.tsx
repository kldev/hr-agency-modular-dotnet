import { useQueryClient } from "@tanstack/react-query";
import { BriefcaseBusiness } from "lucide-react";
import { useCallback } from "react";
import { getPositionSuggestions } from "@/api/endpoints";
import type { PositionSuggestion } from "@/api/models";
import { suggestionKeys } from "@/api/query-keys";
import { usePositionSuggestion } from "@/hooks";
import { SuggestionPicker, type SuggestionPickerProps } from "./SuggestionPicker";

export type PositionsPickerProps = Omit<
	SuggestionPickerProps<PositionSuggestion>,
	"loadSuggestions" | "selectedItem" | "getKey" | "getLabel"
> & {
	/**
	 * The delivery whose roles are on offer. A role belongs to one project, so a picker that
	 * offered every role would be offering choices the backend refuses - and the suggestion API is
	 * scoped this way for the same reason.
	 */
	projectId: string;
};

const getKey = (item: PositionSuggestion) => item.id;
const getLabel = (item: PositionSuggestion) => item.name;

/** How full the role is. Null planned headcount reads as "nobody said", not as "none needed". */
const staffing = (item: PositionSuggestion) =>
	item.plannedHeadcount === null || item.plannedHeadcount === undefined
		? `${item.assignedCount} assigned`
		: `${item.assignedCount}/${item.plannedHeadcount} assigned`;

export function PositionsPicker({ projectId, onChange, ...props }: PositionsPickerProps) {
	const queryClient = useQueryClient();

	const searchPositions = useCallback(
		async (query: string, signal: AbortSignal): Promise<PositionSuggestion[]> =>
			projectId
				? await getPositionSuggestions({ projectId, search: query ?? "" }, undefined, signal)
				: [],
		[projectId],
	);

	const { data: selectedPosition } = usePositionSuggestion(props.value ?? "");

	const handleChange = useCallback(
		(value: string | null, item?: PositionSuggestion) => {
			if (item) {
				queryClient.setQueryData(suggestionKeys.position(item.id), item);
			}

			onChange(value, item);
		},
		[onChange, queryClient],
	);

	return (
		<SuggestionPicker<PositionSuggestion>
			{...props}
			placeholder={props.placeholder ?? "Search roles on this project..."}
			allowCustomValue={false}
			disabled={props.disabled || !projectId}
			onChange={handleChange}
			loadSuggestions={searchPositions}
			selectedItem={selectedPosition ?? null}
			getKey={getKey}
			getLabel={getLabel}
			getDescription={staffing}
			renderItem={(item, selected) => (
				<div className="suggestion-picker-default-item">
					<div className="suggestion-picker-avatar">
						<BriefcaseBusiness size={16} />
					</div>

					<div className="suggestion-picker-item-content">
						<div className="suggestion-picker-item-label">{item.name}</div>

						<div className="suggestion-picker-company-meta">
							<span>{staffing(item)}</span>
						</div>
					</div>

					{selected && <span className="suggestion-picker-selected-mark">✓</span>}
				</div>
			)}
		/>
	);
}
