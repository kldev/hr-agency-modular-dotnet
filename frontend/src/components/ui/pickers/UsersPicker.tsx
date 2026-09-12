import { Mail, UserRound } from "lucide-react";
import { getUsersSuggestions } from "@/api/endpoints";
import type { OrganizationRole, UserSuggestion } from "@/api/models";
import { SuggestionPicker, type SuggestionPickerProps } from "./SuggestionPicker";

type OwnProps = {
	role?: OrganizationRole;
};
export type UsersPickerProps = Omit<
	SuggestionPickerProps<UserSuggestion>,
	"loadSuggestions" | "getKey" | "getLabel"
>;

type Props = OwnProps & UsersPickerProps;

export function UsersPicker(props: Props) {
	const searchUsers = async (query: string, signal: AbortSignal): Promise<UserSuggestion[]> => {
		return await getUsersSuggestions(
			{ search: query ?? "", roles: props.role ? [props.role] : [] },
			undefined,
			signal,
		);
	};

	return (
		<SuggestionPicker<UserSuggestion>
			{...props}
			placeholder={props.placeholder ?? "Search recruiter..."}
			loadSuggestions={searchUsers}
			getKey={(user) => user.id}
			getLabel={(user) => user.fullName}
			getDescription={(user) => user.email}
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
