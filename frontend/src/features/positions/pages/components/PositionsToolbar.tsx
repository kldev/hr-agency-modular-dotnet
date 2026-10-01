import { Input, Switch } from "@heroui/react";
import { Plus, Search, SlidersHorizontal } from "lucide-react";
import type { WorkerContractType } from "@/api/models";
import { Button, EnumFilter } from "@/components/ui";
import { workerContractTypes } from "../../types";

interface PositionsToolbarProps {
	search: string;
	contractType: WorkerContractType | null;
	includeArchived: boolean;
	onSearchChange: (value: string) => void;
	onContractTypeChange: (value: WorkerContractType | null) => void;
	onIncludeArchivedChange: (value: boolean) => void;
	onClear: () => void;
	onAdd: () => void;
}

export function PositionsToolbar({
	search,
	contractType,
	includeArchived,
	onSearchChange,
	onContractTypeChange,
	onIncludeArchivedChange,
	onClear,
	onAdd,
}: PositionsToolbarProps) {
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

						<span className="sr-only">Search positions</span>

						<Input
							value={search}
							placeholder="Search positions"
							onChange={(event) => onSearchChange(event.target.value)}
						/>
					</span>

					{/*
					 * Archived roles are half the list on a long-running client, so they are off by
					 * default and asked for explicitly rather than filtered out of a full list.
					 */}
					<span className="toolbar-toggle">
						<Switch
							id="positions-include-archived"
							isSelected={includeArchived}
							onChange={(selected) => onIncludeArchivedChange(selected)}
						>
							<Switch.Content>
								<Switch.Control>
									<Switch.Thumb />
								</Switch.Control>
							</Switch.Content>
						</Switch>

						<label htmlFor="positions-include-archived">Include archived</label>
					</span>

					<Button
						variant="ghost"
						icon={<SlidersHorizontal size={15} />}
						aria-label="Clear"
						title="Clear"
						onPress={onClear}
					>
						Clear
					</Button>
				</div>

				<div className="toolbar-right">
					<Button variant="primary" icon={<Plus size={15} />} onPress={onAdd}>
						Add position
					</Button>
				</div>
			</div>

			<EnumFilter
				value={contractType}
				options={workerContractTypes}
				onChange={onContractTypeChange}
			/>
		</>
	);
}
