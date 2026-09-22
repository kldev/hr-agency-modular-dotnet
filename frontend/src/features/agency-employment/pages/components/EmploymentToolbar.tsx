import { Plus, Search } from "lucide-react";
import { Button, Input, Toggle } from "@/components/ui";

interface Props {
	search: string;
	coveredOnly: boolean;
	onSearchChange: (value: string) => void;
	onCoveredOnlyChange: (value: boolean) => void;
	onAdd: () => void;
}

export function EmploymentToolbar({
	search,
	coveredOnly,
	onSearchChange,
	onCoveredOnlyChange,
	onAdd,
}: Props) {
	return (
		<div className="toolbar">
			<div className="toolbar-left">
				<span className="search-input">
					<Search
						size={15}
						aria-hidden="true"
						style={{ position: "absolute", left: 12, top: 10 }}
					/>

					<span className="sr-only">Search people</span>

					<Input
						value={search}
						placeholder="Search by name or e-mail"
						onChange={(event) => onSearchChange(event.target.value)}
					/>
				</span>

				{/*
				 * The one filter that answers the question the register is kept for: who owes hours.
				 * It is the contract type read through the same rule the backend applies, not a
				 * second question.
				 */}
				<span className="toolbar-toggle">
					<Toggle
						id="employment-covered-only"
						checked={coveredOnly}
						onChange={(event) => onCoveredOnlyChange(event.target.checked)}
					/>

					<label htmlFor="employment-covered-only">Owes hours</label>
				</span>
			</div>

			<div className="toolbar-right">
				<Button variant="primary" icon={<Plus size={15} />} onClick={onAdd}>
					Record employment
				</Button>
			</div>
		</div>
	);
}
