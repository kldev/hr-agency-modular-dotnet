import { useQueryClient } from "@tanstack/react-query";
import { FolderKanban } from "lucide-react";
import { useCallback } from "react";
import { getProjectSuggestions } from "@/api/endpoints";
import type { ProjectSuggestion } from "@/api/models";
import { suggestionKeys } from "@/api/query-keys";
import { projectStatuses } from "@/features/projects/types";
import { useProjectSuggestion } from "@/hooks";
import { SuggestionPicker, type SuggestionPickerProps } from "./SuggestionPicker";

export type ProjectsPickerProps = Omit<
	SuggestionPickerProps<ProjectSuggestion>,
	"loadSuggestions" | "selectedItem" | "getKey" | "getLabel"
>;

type Props = ProjectsPickerProps;

const getKey = (item: ProjectSuggestion) => item.id;
const getLabel = (item: ProjectSuggestion) => item.name;
const getDescription = (item: ProjectSuggestion) =>
	`${item.companyName}, ${projectStatuses[item.status]}`;

export function ProjectsPicker({ onChange, ...props }: Props) {
	const queryClient = useQueryClient();

	const searchProjects = useCallback(
		async (query: string, signal: AbortSignal): Promise<ProjectSuggestion[]> =>
			await getProjectSuggestions({ search: query ?? "" }, undefined, signal),
		[],
	);

	const { data: selectedProject } = useProjectSuggestion(props.value ?? "");

	const handleChange = useCallback(
		(value: string | null, item?: ProjectSuggestion) => {
			if (item) {
				queryClient.setQueryData(suggestionKeys.project(item.id), item);
			}

			onChange(value, item);
		},
		[onChange, queryClient],
	);

	return (
		<SuggestionPicker<ProjectSuggestion>
			{...props}
			placeholder={props.placeholder ?? "Search projects..."}
			allowCustomValue={props.allowCustomValue ?? false}
			onChange={handleChange}
			loadSuggestions={searchProjects}
			selectedItem={selectedProject ?? null}
			getKey={getKey}
			getLabel={getLabel}
			getDescription={getDescription}
			renderItem={(item, selected) => (
				<div className="suggestion-picker-default-item">
					<div className="suggestion-picker-avatar">
						<FolderKanban size={16} />
					</div>

					<div className="suggestion-picker-item-content">
						<div className="suggestion-picker-item-label">{item.name}</div>

						<div className="suggestion-picker-company-meta">
							<span>
								{item.companyName}, {projectStatuses[item.status]}
							</span>
						</div>
					</div>

					{selected && <span className="suggestion-picker-selected-mark">✓</span>}
				</div>
			)}
		/>
	);
}
