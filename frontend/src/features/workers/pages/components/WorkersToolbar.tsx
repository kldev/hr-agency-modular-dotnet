import { Plus, Search, SlidersHorizontal } from "lucide-react";
import type { WorkerStatus } from "@/api/models";
import { Button, EnumFilter, type ViewMode, ViewSwitch } from "@/components/ui";
import { Input } from "@/components/ui/Input";
import { workerStatuses } from "../../types";

interface WorkersToolbarProps {
	search: string;
	status: WorkerStatus | null;
	onSearchChange: (value: string) => void;
	onStatusChange: (value: WorkerStatus | null) => void;
	onClear: () => void;
	onAdd: () => void;
	view: ViewMode;
	onViewChange: (view: ViewMode) => void;
}

/*
 * One row of chips, and it is the pipeline. The desk was a second row next to it, and a second row
 * is a second question - except it was the same question: the department is `OwnerOf(status)` on the
 * backend, so "Legalisation" appeared twice on screen meaning the same set of people. Who looks
 * after which stage is something everybody in the office knows without being told by a filter.
 */
export function WorkersToolbar({
	search,
	status,
	onSearchChange,
	onStatusChange,
	onClear,
	onAdd,
	view,
	onViewChange,
}: WorkersToolbarProps) {
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

						<span className="sr-only">Search people by name</span>

						{/* Names only. The API deliberately does not search document numbers, so that one
						    never lands in a query string, a browser history or a server log. */}
						<Input
							value={search}
							placeholder="Search by name"
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

				<div className="toolbar-right">
					<ViewSwitch view={view} onChange={onViewChange} />

					<Button variant="primary" icon={<Plus size={15} />} onClick={onAdd}>
						Register worker
					</Button>
				</div>
			</div>

			{/* on the board every status is a column already */}
			{view === "table" ? (
				<EnumFilter value={status} options={workerStatuses} onChange={onStatusChange} />
			) : null}
		</>
	);
}
