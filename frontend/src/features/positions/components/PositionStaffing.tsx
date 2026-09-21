interface PositionStaffingProps {
	assigned: number | string;
	planned: number | string | null | undefined;
}

/**
 * How full a role is. A missing target headcount is not "none missing" — nobody said how many it
 * needs — so it reads as a bare count rather than a fraction, and no shortfall is claimed.
 */
export function PositionStaffing({ assigned, planned }: PositionStaffingProps) {
	const assignedCount = Number(assigned ?? 0);

	if (planned === null || planned === undefined || planned === "") {
		return <span className="table-figure">{assignedCount}</span>;
	}

	const plannedCount = Number(planned);
	const missing = Math.max(0, plannedCount - assignedCount);

	return (
		<span className="table-figure">
			{assignedCount}/{plannedCount}
			{missing > 0 ? <span className="badge badge-warning ml-2">{missing} short</span> : null}
		</span>
	);
}
