import { PlusIcon } from "lucide-react";
import { Button } from "../Button";
import "./list-section.css";

export function DetailsListSection({
	title,
	items,
	className = "",
	onAdd,
}: {
	title: string;
	items: string[];
	className?: string;
	onAdd?: () => void;
}) {
	return (
		<section className={`data-content-section ${className}`}>
			<header className="data-content-header">
				<h2>{title}</h2>
				<div className="flex flex-row gap-2 items-center justify-end">
					{onAdd ? (
						<Button variant="ghost" onClick={onAdd} icon={<PlusIcon size={16} />}></Button>
					) : null}
					<span className="data-details-list-count">{items.length}</span>
				</div>
			</header>

			{items.length === 0 ? (
				<div className="data-content-empty">No items added.</div>
			) : (
				<ul className="data-content-list">
					{items.map((item, index) => (
						<li key={`${item}-${index}`}>{item}</li>
					))}
				</ul>
			)}
		</section>
	);
}
