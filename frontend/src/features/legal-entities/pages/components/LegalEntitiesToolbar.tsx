import { Input, Switch } from "@heroui/react";
import { Plus, Search } from "lucide-react";
import { Button } from "@/components/ui";

interface LegalEntitiesToolbarProps {
	search: string;
	activeOnly: boolean;
	onSearchChange: (value: string) => void;
	onActiveOnlyChange: (value: boolean) => void;
	onAdd: () => void;
}

export function LegalEntitiesToolbar({
	search,
	activeOnly,
	onSearchChange,
	onActiveOnlyChange,
	onAdd,
}: LegalEntitiesToolbarProps) {
	return (
		<div className="toolbar">
			<div className="toolbar-left">
				<span className="search-input">
					<Search
						size={15}
						aria-hidden="true"
						style={{ position: "absolute", left: 12, top: 10 }}
					/>

					<span className="sr-only">Search legal entities</span>

					<Input
						value={search}
						placeholder="Search by name or tax ID"
						onChange={(event) => onSearchChange(event.target.value)}
					/>
				</span>

				<span className="toolbar-toggle">
					<Switch
						id="legal-entities-active-only"
						isSelected={activeOnly}
						onChange={(selected) => onActiveOnlyChange(selected)}
					>
						<Switch.Content>
							<Switch.Control>
								<Switch.Thumb />
							</Switch.Control>
						</Switch.Content>
					</Switch>

					<label htmlFor="legal-entities-active-only">Trading only</label>
				</span>
			</div>

			<div className="toolbar-right">
				<Button variant="primary" icon={<Plus size={15} />} onPress={onAdd}>
					New entity
				</Button>
			</div>
		</div>
	);
}
