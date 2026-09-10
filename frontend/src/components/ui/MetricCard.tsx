interface MetricsProps {
	children: React.ReactNode;
	columns?: number;
}

export function Metrics({ children, columns = 5 }: MetricsProps) {
	return (
		<section
			className="metrics-cards"
			style={{ "--metrics-columns": columns } as React.CSSProperties}
		>
			{children}
		</section>
	);
}

interface MetricCardProps {
	icon: React.ReactNode;
	label: string;
	value: number;
	className?: string;
}

export function MetricCard({ label, icon, value, className = "" }: MetricCardProps) {
	return (
		<div className={`metric-card ${className}`}>
			<div className="metric-card-icon">{icon}</div>

			<div>
				<div className="metric-card-value">{value}</div>

				<div className="metric-card-label">{label}</div>
			</div>
		</div>
	);
}
