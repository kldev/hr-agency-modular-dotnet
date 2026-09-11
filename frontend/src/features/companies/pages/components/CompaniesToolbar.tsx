import { Plus, Search, SlidersHorizontal } from "lucide-react";

import { Button } from "@/components/ui";
import { Input } from "@/components/ui/Input";

interface CompaniesToolbarProps {
	search: string;
	onSearchChange: (value: string) => void;
	onClear: () => void;
	onAdd: () => void;
}

export function CompaniesToolbar({
	search,
	onSearchChange,
	onAdd,
	onClear,
}: CompaniesToolbarProps) {
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

					<span className="sr-only">Search users</span>

					<Input
						value={search}
						placeholder="Search companies"
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
					Add company
				</Button>
			</div>
		</div>
	);
}
