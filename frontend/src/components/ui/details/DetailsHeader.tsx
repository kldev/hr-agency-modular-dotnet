import { Pencil } from "lucide-react";
import { ActionButton, ItemMark } from "@/components/ui";

interface dataDetailsHeaderProps {
	name: string;
	website?: string;
	onEdit?: () => void;
	detailsAddons?: React.ReactNode;
	extraAdd?: React.ReactNode;
}

function renderWebiste(website?: string) {
	if (!website) return null;
	return (
		<a href={website} target="_blank" rel="noreferrer" className="data-details-website">
			{website}
		</a>
	);
}

export function DetailsHeader({
	name,
	onEdit,
	website,
	detailsAddons,
	extraAdd,
}: dataDetailsHeaderProps) {
	return (
		<header className="data-details-header">
			<div className="data-details-header-main">
				<ItemMark name={name} />

				<div className="data-details-title">
					<h1>{name}</h1>
					{renderWebiste(website)}
					{detailsAddons}
				</div>
			</div>

			<div className="flex flex-row gap-3">
				{extraAdd}
				{onEdit ? (
					<div className="data-details-header-actions">
						<ActionButton title="Edit data" onClick={onEdit}>
							<Pencil size={15} />
							<span>Edit</span>
						</ActionButton>
					</div>
				) : null}
			</div>
		</header>
	);
}
