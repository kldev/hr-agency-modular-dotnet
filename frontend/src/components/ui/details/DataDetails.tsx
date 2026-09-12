import { Plus } from "lucide-react";
import { Button } from "../Button";
import "./details.css";

export function DetailItem({ label, children }: { label: string; children: React.ReactNode }) {
	return (
		<div className="data-detail-item">
			<dt>{label}</dt>
			<dd>{children || "—"}</dd>
		</div>
	);
}

export function DetailOverviewHeader({
	title,
	description,
	onAdd,
}: {
	title: string;
	description: string;
	onAdd?: () => void;
}) {
	return (
		<div className="data-details-section-header">
			<div>
				<h2>{title}</h2>
				<p>{description}</p>
			</div>
			{onAdd ? (
				<div className="toolbar-right">
					<Button variant="ghost" icon={<Plus size={15} />} onClick={onAdd}></Button>
				</div>
			) : null}
		</div>
	);
}

type DataDetailsProps = {
	children: React.ReactNode;
};
export function DataDetails({ children }: DataDetailsProps) {
	return <main className="data-details">{children}</main>;
}
