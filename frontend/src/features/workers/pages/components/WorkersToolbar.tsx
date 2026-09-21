import { Plus, Search, SlidersHorizontal } from "lucide-react";
import type { ResponsibleDepartment, WorkerStatus } from "@/api/models";
import { Button, EnumFilter } from "@/components/ui";
import { Input } from "@/components/ui/Input";
import { responsibleDepartments, workerStatuses } from "../../types";

/** `None` is what a terminated person's department is; nobody filters a queue by "nobody's". */
const departmentFilterOptions: Record<string, string> = Object.fromEntries(
	Object.entries(responsibleDepartments).filter(([key]) => key !== "None"),
);

interface WorkersToolbarProps {
	search: string;
	status: WorkerStatus | null;
	department: ResponsibleDepartment | null;
	onSearchChange: (value: string) => void;
	onStatusChange: (value: WorkerStatus | null) => void;
	onDepartmentChange: (value: ResponsibleDepartment | null) => void;
	onClear: () => void;
	onAdd: () => void;
}

export function WorkersToolbar({
	search,
	status,
	department,
	onSearchChange,
	onStatusChange,
	onDepartmentChange,
	onClear,
	onAdd,
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
					<Button variant="primary" icon={<Plus size={15} />} onClick={onAdd}>
						Register worker
					</Button>
				</div>
			</div>

			<EnumFilter value={status} options={workerStatuses} onChange={onStatusChange} />

			<EnumFilter
				value={department}
				options={departmentFilterOptions}
				allLabel="Every desk"
				onChange={(value) => onDepartmentChange((value as ResponsibleDepartment) ?? null)}
			/>
		</>
	);
}
