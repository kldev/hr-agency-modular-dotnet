import "./empty-state.css";

interface EmptyStateProps {
	children: React.ReactNode;
	title: string;
	description?: string;
}

export function EmptyState({
	children,
	title,
	description = "Try changing the search or filter criteria.",
}: EmptyStateProps) {
	return (
		<div className="empty-state">
			{children}
			<div className="empty-state-title">{title}</div>
			<p className="empty-state-description">{description}</p>
		</div>
	);
}
