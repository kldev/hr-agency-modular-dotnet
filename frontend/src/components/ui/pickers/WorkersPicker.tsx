import { useQueryClient } from "@tanstack/react-query";
import { HardHat } from "lucide-react";
import { useCallback } from "react";
import { getWorkerSuggestions } from "@/api/endpoints";
import type { WorkerSuggestion } from "@/api/models";
import { suggestionKeys } from "@/api/query-keys";
import { workerStatuses } from "@/features/workers/types";
import { useWorkerSuggestion } from "@/hooks";
import { SuggestionPicker, type SuggestionPickerProps } from "./SuggestionPicker";

export type WorkersPickerProps = Omit<
	SuggestionPickerProps<WorkerSuggestion>,
	"loadSuggestions" | "selectedItem" | "getKey" | "getLabel"
>;

type Props = WorkersPickerProps;

const getKey = (item: WorkerSuggestion) => item.id;
const getLabel = (item: WorkerSuggestion) => item.fullName;
const getDescription = (item: WorkerSuggestion) => workerStatuses[item.status];

export function WorkersPicker({ onChange, ...props }: Props) {
	const queryClient = useQueryClient();

	const searchWorkers = useCallback(
		async (query: string, signal: AbortSignal): Promise<WorkerSuggestion[]> =>
			await getWorkerSuggestions({ search: query ?? "" }, undefined, signal),
		[],
	);

	/*
	 * Resolves `value` back into a person whenever the picker is mounted without a label - a wizard
	 * step the user navigated back to, or an edit form seeded from a record.
	 */
	const { data: selectedWorker } = useWorkerSuggestion(props.value ?? "");

	/*
	 * Seeding the cache with the item the dropdown just handed us keeps the query above from firing
	 * a second request for somebody we already have.
	 */
	const handleChange = useCallback(
		(value: string | null, item?: WorkerSuggestion) => {
			if (item) {
				queryClient.setQueryData(suggestionKeys.worker(item.id), item);
			}

			onChange(value, item);
		},
		[onChange, queryClient],
	);

	return (
		<SuggestionPicker<WorkerSuggestion>
			{...props}
			placeholder={props.placeholder ?? "Search people..."}
			allowCustomValue={props.allowCustomValue ?? false}
			onChange={handleChange}
			loadSuggestions={searchWorkers}
			selectedItem={selectedWorker ?? null}
			getKey={getKey}
			getLabel={getLabel}
			getDescription={getDescription}
			renderItem={(item, selected) => (
				<div className="suggestion-picker-default-item">
					<div className="suggestion-picker-avatar">
						<HardHat size={16} />
					</div>

					<div className="suggestion-picker-item-content">
						<div className="suggestion-picker-item-label">{item.fullName}</div>

						<div className="suggestion-picker-company-meta">
							<span>{workerStatuses[item.status]}</span>
						</div>
					</div>

					{selected && <span className="suggestion-picker-selected-mark">✓</span>}
				</div>
			)}
		/>
	);
}
