import { Plus, Search, SlidersHorizontal } from "lucide-react";

import { Button } from "@/components/ui";
import { Input } from "@/components/ui/Input";

interface OrganizationsToolbarProps {
	search: string;
	onSearchChange: (value: string) => void;
	onClear: () => void;
	onAdd: () => void;
}

export function OrganizationsToolbar({
	search,
	onSearchChange,
	onAdd,
	onClear,
}: OrganizationsToolbarProps) {
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

					<span className="sr-only">Search organizations</span>

					<Input
						value={search}
						placeholder="Search organizations"
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
					Add organization
				</Button>
			</div>
		</div>
	);
}
