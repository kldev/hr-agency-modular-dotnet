import { useQueryClient } from "@tanstack/react-query";
import { Users } from "lucide-react";
import { useCallback } from "react";
import { getTeamsSuggestions } from "@/api/endpoints";
import type { TeamSuggestion } from "@/api/models";
import { suggestionKeys } from "@/api/query-keys";
import { useTeamSuggestion } from "@/hooks";
import { SuggestionPicker, type SuggestionPickerProps } from "./SuggestionPicker";

export type TeamsPickerProps = Omit<
	SuggestionPickerProps<TeamSuggestion>,
	"loadSuggestions" | "selectedItem" | "getKey" | "getLabel"
>;

const getKey = (team: TeamSuggestion) => team.id;
const getLabel = (team: TeamSuggestion) => team.name;

const memberCount = (team: TeamSuggestion) => {
	const count = Number(team.memberCount);

	return count === 1 ? "1 member" : `${count} members`;
};

export function TeamsPicker({ onChange, ...props }: TeamsPickerProps) {
	const queryClient = useQueryClient();

	const searchTeams = useCallback(
		async (query: string, signal: AbortSignal): Promise<TeamSuggestion[]> => {
			return await getTeamsSuggestions({ search: query ?? "" }, undefined, signal);
		},
		[],
	);

	/*
	 * Resolves `value` back into a team whenever the picker is mounted without a label - an edit
	 * form seeded from a record, where the form holds an id and nothing else.
	 */
	const { data: selectedTeam } = useTeamSuggestion(props.value ?? "");

	/*
	 * Seeding the cache with the item the dropdown just handed us keeps the query above from firing
	 * a second request for a team we already have.
	 */
	const handleChange = useCallback(
		(value: string | null, item?: TeamSuggestion) => {
			if (item) {
				queryClient.setQueryData(suggestionKeys.team(item.id), item);
			}

			onChange(value, item);
		},
		[onChange, queryClient],
	);

	return (
		<SuggestionPicker<TeamSuggestion>
			{...props}
			placeholder={props.placeholder ?? "Search teams ..."}
			allowCustomValue={props.allowCustomValue ?? false}
			onChange={handleChange}
			loadSuggestions={searchTeams}
			selectedItem={selectedTeam ?? null}
			getKey={getKey}
			getLabel={getLabel}
			getDescription={memberCount}
			renderItem={(team, selected) => (
				<div className="suggestion-picker-default-item">
					<div className="suggestion-picker-avatar">
						<Users size={16} />
					</div>

					<div className="suggestion-picker-item-content">
						<div className="suggestion-picker-item-label">{team.name}</div>

						<div className="suggestion-picker-company-meta">
							<span>{memberCount(team)}</span>
						</div>
					</div>

					{selected && <span className="suggestion-picker-selected-mark">✓</span>}
				</div>
			)}
		/>
	);
}
