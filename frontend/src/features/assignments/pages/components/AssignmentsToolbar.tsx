import { Search, SlidersHorizontal } from "lucide-react";
import type { AssignmentStatus } from "@/api/models";
import { Button, EnumFilter } from "@/components/ui";
import { Input } from "@/components/ui/Input";
import { assignmentStatuses } from "../../types";

interface AssignmentsToolbarProps {
	search: string;
	status: AssignmentStatus | null;
	onSearchChange: (value: string) => void;
	onStatusChange: (value: AssignmentStatus | null) => void;
	onClear: () => void;
}

/*
 * No "Add" button: a posting is planned from the person or from the project, because both of them
 * have to be picked and one of the two is always already in hand.
 */
export function AssignmentsToolbar({
	search,
	status,
	onSearchChange,
	onStatusChange,
	onClear,
}: AssignmentsToolbarProps) {
	return (
		<>
			<div className="toolbar">
				<div className="toolbar-left">
					<span className="search-input">
						<Search
							size={15}
							aria-hidden="true"
							style={{
								position: "absolute",
								left: 12,
								top: 10,
							}}
						/>

						<span className="sr-only">Search assignments</span>

						<Input
							value={search}
							placeholder="Search by person or project"
							onChange={(event) => onSearchChange(event.target.value)}
						/>
					</span>

					<Button
						variant="ghost"
						icon={<SlidersHorizontal size={15} />}
						aria-label="Clear"
						title="Clear"
						onClick={onClear}
					>
						Clear
					</Button>
				</div>
			</div>

			<EnumFilter value={status} options={assignmentStatuses} onChange={onStatusChange} />
		</>
	);
}
