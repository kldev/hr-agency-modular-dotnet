import { Mail, Phone, Plus } from "lucide-react";
import { Button } from "../Button";
import "./details.css";

export function EmailItem({ email }: { email: string }) {
	return (
		<DetailItem label="Email">
			<div className="flex flex-1 gap-2 items-center">
				<Mail size={12} />
				<a href={`mailto:${email}`}>{email}</a>
			</div>
		</DetailItem>
	);
}

export function PhoneItem({ phone }: { phone?: string }) {
	return (
		<DetailItem label="Phone">
			<div className="flex flex-1 gap-2 items-center">
				<Phone size={12} />
				<a href={`tel:${phone}`}>{phone ?? "-"}</a>
			</div>
		</DetailItem>
	);
}

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
	className,
}: {
	title: string;
	description: string;
	onAdd?: () => void;
	className?: string;
}) {
	return (
		<div className={`data-details-section-header ${className}`}>
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

export function DataDetailsLayout({
	main,
	sidebar,
}: {
	main: React.ReactNode;
	sidebar: React.ReactNode;
}) {
	return (
		<div className="data-details-layout">
			<div className="data-details-main">{main}</div>
			<aside className="data-details-sidebar">{sidebar}</aside>
		</div>
	);
}
