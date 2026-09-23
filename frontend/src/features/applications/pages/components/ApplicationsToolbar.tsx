import { Search, SlidersHorizontal } from "lucide-react";
import type { CandidateSource } from "@/api/models";
import { Button, EnumSelectFilter, type ViewMode, ViewSwitch } from "@/components/ui";
import { Input } from "@/components/ui/Input";
import { applicationSources, type WorkerFileFilter, workerFileFilters } from "../../types";

interface ApplicationsToolbarProps {
	search: string;
	source: CandidateSource | null;

	onSearchChange: (value: string) => void;
	onSourceChange: (value: CandidateSource | null) => void;
	worker: WorkerFileFilter | null;
	onWorkerChange: (value: WorkerFileFilter | null) => void;
	companyId?: string;
	onCompanyChange?: (value: string) => void;
	onClear: () => void;
	view: ViewMode;
	onViewChange: (view: ViewMode) => void;
}

export function ApplicationsToolbar({
	search,
	source,
	onSearchChange,
	onSourceChange,
	worker,
	onWorkerChange,
	onClear,
	view,
	onViewChange,
}: ApplicationsToolbarProps) {
	return (
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

					<span className="sr-only">Search applications</span>

					<Input
						value={search}
						placeholder="Search applicants"
						onChange={(event) => onSearchChange(event.target.value)}
					/>
				</span>

				<EnumSelectFilter
					options={applicationSources}
					value={source}
					onChange={(value) => onSourceChange(value ?? null)}
				/>

				<EnumSelectFilter
					options={workerFileFilters}
					value={worker}
					allLabel="Any worker file"
					onChange={(value) => onWorkerChange(value ?? null)}
				/>

				<Button
					className="min-w-25"
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
			</div>
		</div>
	);
}
