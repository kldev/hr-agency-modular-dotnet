import { useQuery, useQueryClient } from "@tanstack/react-query";
import { FileUser } from "lucide-react";
import { useCallback } from "react";
import { getJobApplication, getJobApplicationsSlice } from "@/api/endpoints";
import type { JobApplicationProjection } from "@/api/models";
import { applicationKeys } from "@/api/query-keys";
import { SuggestionPicker, type SuggestionPickerProps } from "./SuggestionPicker";

const SUGGESTION_LIMIT = 25;
const SUGGESTION_STALE_TIME = 5 * 60 * 1000;

export type ApplicationsPickerProps = Omit<
	SuggestionPickerProps<JobApplicationProjection>,
	"loadSuggestions" | "selectedItem" | "getKey" | "getLabel"
>;

type Props = ApplicationsPickerProps;

const getKey = (item: JobApplicationProjection) => item.id;
const getLabel = (item: JobApplicationProjection) => item.applicantFullName;
const getDescription = (item: JobApplicationProjection) => item.jobPostTitle;

/**
 * There is no suggestion endpoint for applications, so this searches the ordinary slice and
 * resolves a preselected one through `getJobApplication`. A full projection is more than a picker
 * needs, but it is also exactly what the caller wants next: the registration wizard prefills the
 * person's name, e-mail and phone from it rather than making somebody retype what we already know.
 */
export function ApplicationsPicker({ onChange, ...props }: Props) {
	const queryClient = useQueryClient();

	const searchApplications = useCallback(
		async (query: string, signal: AbortSignal): Promise<JobApplicationProjection[]> => {
			const slice = await getJobApplicationsSlice(
				{ search: query ?? "", page: 1, pageSize: SUGGESTION_LIMIT },
				undefined,
				signal,
			);

			return slice.content ?? [];
		},
		[],
	);

	const { data: selectedApplication } = useQuery({
		queryKey: applicationKeys.detail(props.value ?? ""),
		queryFn: ({ signal }) => getJobApplication(props.value ?? "", undefined, signal),
		enabled: Boolean(props.value),
		staleTime: SUGGESTION_STALE_TIME,
		retry: false,
	});

	const handleChange = useCallback(
		(value: string | null, item?: JobApplicationProjection) => {
			if (item) {
				queryClient.setQueryData(applicationKeys.detail(item.id), item);
			}

			onChange(value, item);
		},
		[onChange, queryClient],
	);

	return (
		<SuggestionPicker<JobApplicationProjection>
			{...props}
			placeholder={props.placeholder ?? "Search applications..."}
			allowCustomValue={props.allowCustomValue ?? false}
			onChange={handleChange}
			loadSuggestions={searchApplications}
			selectedItem={selectedApplication ?? null}
			getKey={getKey}
			getLabel={getLabel}
			getDescription={getDescription}
			renderItem={(item, selected) => (
				<div className="suggestion-picker-default-item">
					<div className="suggestion-picker-avatar">
						<FileUser size={16} />
					</div>

					<div className="suggestion-picker-item-content">
						<div className="suggestion-picker-item-label">{item.applicantFullName}</div>

						<div className="suggestion-picker-company-meta">
							<span>
								{item.jobPostTitle}
								{item.applicantEmail ? ` · ${item.applicantEmail}` : ""}
							</span>
						</div>
					</div>

					{selected && <span className="suggestion-picker-selected-mark">✓</span>}
				</div>
			)}
		/>
	);
}
