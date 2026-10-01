import { Input } from "@heroui/react";
import { Plus, Search, SlidersHorizontal } from "lucide-react";
import { Button } from "@/components/ui";

interface UsersToolbarProps {
	search: string;
	onSearchChange: (value: string) => void;
	onClear: () => void;
	onAdd?: () => void;
}

export function UsersToolbar({ search, onSearchChange, onAdd, onClear }: UsersToolbarProps) {
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
						placeholder="Search users"
						onChange={(event) => onSearchChange(event.target.value)}
					/>
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

			{onAdd ? (
				<div className="toolbar-right">
					<Button variant="primary" icon={<Plus size={15} />} onPress={onAdd}>
						Add user
					</Button>
				</div>
			) : null}
		</div>
	);
}
