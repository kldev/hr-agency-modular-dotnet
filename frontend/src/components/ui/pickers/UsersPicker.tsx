import { useQueryClient } from "@tanstack/react-query";
import { Mail, UserRound } from "lucide-react";
import { useCallback } from "react";
import { getUsersSuggestions } from "@/api/endpoints";
import type { OrganizationRole, UserSuggestion } from "@/api/models";
import { suggestionKeys } from "@/api/query-keys";
import { useUserSuggestion } from "@/hooks";
import { SuggestionPicker, type SuggestionPickerProps } from "./SuggestionPicker";

type OwnProps = {
	role?: OrganizationRole;
};
export type UsersPickerProps = Omit<
	SuggestionPickerProps<UserSuggestion>,
	"loadSuggestions" | "selectedItem" | "getKey" | "getLabel"
>;

type Props = OwnProps & UsersPickerProps;

const getKey = (user: UserSuggestion) => user.id;
const getLabel = (user: UserSuggestion) => user.fullName;
const getDescription = (user: UserSuggestion) => user.email;

export function UsersPicker({ role, onChange, ...props }: Props) {
	const queryClient = useQueryClient();

	const searchUsers = useCallback(
		async (query: string, signal: AbortSignal): Promise<UserSuggestion[]> => {
			return await getUsersSuggestions(
				{ search: query ?? "", roles: role ? [role] : [] },
				undefined,
				signal,
			);
		},
		[role],
	);

	/*
	 * Resolves `value` back into a user whenever the picker is mounted without a label - a wizard
	 * step the user navigated back to, or an edit form seeded from a record.
	 *
	 * Resolving by id intentionally ignores `role` - the user stored on the aggregate has to show up
	 * even when their role changed after they were assigned.
	 */
	const { data: selectedUser } = useUserSuggestion(props.value ?? "");

	/*
	 * Seeding the cache with the item the dropdown just handed us keeps the query above from firing
	 * a second request for a user we already have.
	 */
	const handleChange = useCallback(
		(value: string | null, item?: UserSuggestion) => {
			if (item) {
				queryClient.setQueryData(suggestionKeys.user(item.id), item);
			}

			onChange(value, item);
		},
		[onChange, queryClient],
	);

	return (
		<SuggestionPicker<UserSuggestion>
			{...props}
			placeholder={props.placeholder ?? "Search ..."}
			allowCustomValue={props.allowCustomValue ?? false}
			onChange={handleChange}
			loadSuggestions={searchUsers}
			selectedItem={selectedUser ?? null}
			getKey={getKey}
			getLabel={getLabel}
			getDescription={getDescription}
			renderItem={(user, selected) => (
				<div className="suggestion-picker-default-item">
					<div className="suggestion-picker-avatar">
						<UserRound size={16} />
					</div>

					<div className="suggestion-picker-item-content">
						<div className="suggestion-picker-item-label">{user.fullName}</div>

						<div className="suggestion-picker-company-meta">
							<Mail size={12} />

							<span>{user.email}</span>
						</div>
					</div>

					{selected && <span className="suggestion-picker-selected-mark">✓</span>}
				</div>
			)}
		/>
	);
}
