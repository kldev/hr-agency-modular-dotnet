import { Plus, Search, SlidersHorizontal } from "lucide-react";
import type { CandidateSource } from "@/api/models";
import { Button, EnumSelectFilter } from "@/components/ui";
import { Input } from "@/components/ui/Input";
import { applicationSources } from "../../types";

interface ApplicationsToolbarProps {
	search: string;
	source: CandidateSource | null;

	onSearchChange: (value: string) => void;
	onSourceChange: (value: CandidateSource | null) => void;
	companyId?: string;
	onCompanyChange?: (value: string) => void;
	onClear: () => void;
	onAdd: () => void;
}

export function ApplicationsToolbar({
	search,
	source,
	onSearchChange,
	onSourceChange,
	onAdd,
	onClear
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
						placeholder="Search candidates..."
						onChange={(event) => onSearchChange(event.target.value)}
					/>
				</span>

				<EnumSelectFilter
					options={applicationSources}
					value={source}
					onChange={(value) => onSourceChange(value)}
				/>

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
					Add application
				</Button>
			</div>
		</div>
	);
}
