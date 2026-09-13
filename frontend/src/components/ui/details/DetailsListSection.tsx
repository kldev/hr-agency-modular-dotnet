import "./list-section.css";

export function DetailsListSection({
	title,
	items,
	className = "",
}: {
	title: string;
	items: string[];
	className?: string;
}) {
	return (
		<section className={`data-content-section ${className}`}>
			<header className="data-content-header">
				<h2>{title}</h2>
				<span>{items.length}</span>
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
