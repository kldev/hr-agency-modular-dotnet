import { Plus, Search, SlidersHorizontal } from "lucide-react";
import type { ProjectStatus } from "@/api/models";
import { Button, EnumFilter } from "@/components/ui";
import { Input } from "@/components/ui/Input";
import { projectStatuses } from "../../types";

interface ProjectsToolbarProps {
	search: string;
	status: ProjectStatus | null;
	onSearchChange: (value: string) => void;
	onStatusChange: (value: ProjectStatus | null) => void;
	onClear: () => void;
	onAdd: () => void;
}

export function ProjectsToolbar({
	search,
	status,
	onSearchChange,
	onStatusChange,
	onClear,
	onAdd,
}: ProjectsToolbarProps) {
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

						<span className="sr-only">Search projects</span>

						<Input
							value={search}
							placeholder="Search projects"
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
						Add project
					</Button>
				</div>
			</div>

			<EnumFilter value={status} options={projectStatuses} onChange={onStatusChange} />
		</>
	);
}
