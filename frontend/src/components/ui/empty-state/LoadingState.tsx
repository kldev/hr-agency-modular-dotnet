import "./empty-state.css";
export function LoadingState() {
	return (
		<div className="empty-state">
			<span className="spinner" aria-hidden="true" />
			<div className="empty-state-title">Loading...</div>
			<p className="empty-state-description">Retrieving records...</p>
		</div>
	);
}
