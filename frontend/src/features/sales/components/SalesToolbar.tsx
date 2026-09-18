import clsx from "clsx";
import { Columns3, List, Plus, Search, SlidersHorizontal } from "lucide-react";

import { Button, OnlyMine } from "@/components/ui";
import { Input } from "@/components/ui/Input";
import type { SalesView } from "../types";

interface SalesToolbarProps {
	search: string;
	onSearchChange: (value: string) => void;
	companyId?: string;
	onCompanyChange?: (value: string) => void;
	onClear: () => void;
	onAdd: () => void;
	onlyMine: boolean;
	onlyMineOnChange: (val: boolean) => void;
	view: SalesView;
	onViewChange: (view: SalesView) => void;
}

export function SalesToolbar({
	search,
	onSearchChange,
	onAdd,
	onClear,
	onlyMine,
	onlyMineOnChange,
	view,
	onViewChange,
}: SalesToolbarProps) {
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
						placeholder="Search opportunites"
						onChange={(event) => onSearchChange(event.target.value)}
					/>
				</span>
				<OnlyMine onChange={onlyMineOnChange} checked={onlyMine} />
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
				<div className="sales-view-switch shrink-0">
					<Button
						className={clsx(view === "kanban" && "sales-view-active")}
						variant={view === "kanban" ? "secondary" : "ghost"}
						icon={<Columns3 size={15} />}
						aria-pressed={view === "kanban"}
						aria-label="Kanban view"
						title="Kanban view"
						onClick={() => onViewChange("kanban")}
					>
						Kanban
					</Button>

					<Button
						className={clsx(view === "table" && "sales-view-active")}
						variant={view === "table" ? "secondary" : "ghost"}
						icon={<List size={15} />}
						aria-pressed={view === "table"}
						aria-label="Table view"
						title="Table view"
						onClick={() => onViewChange("table")}
					>
						Table
					</Button>
				</div>

				<Button variant="primary" icon={<Plus size={15} />} onClick={onAdd}>
					Add opportunity
				</Button>
			</div>
		</div>
	);
}
