import clsx from "clsx";
import { Columns3, List } from "lucide-react";
import { Button } from "./Button";
import "./view-switch.css";

export type ViewMode = "table" | "kanban";

interface ViewSwitchProps {
	view: ViewMode;
	onChange: (view: ViewMode) => void;
}

const views: { value: ViewMode; label: string; icon: typeof List }[] = [
	{ value: "kanban", label: "Kanban", icon: Columns3 },
	{ value: "table", label: "Table", icon: List },
];

/** The table/kanban toggle of a list page. The page keeps the value in its url. */
export function ViewSwitch({ view, onChange }: ViewSwitchProps) {
	return (
		<div className="view-switch shrink-0">
			{views.map(({ value, label, icon: Icon }) => (
				<Button
					key={value}
					className={clsx(view === value && "view-switch-active")}
					variant={view === value ? "secondary" : "ghost"}
					icon={<Icon size={15} />}
					aria-pressed={view === value}
					aria-label={`${label} view`}
					title={`${label} view`}
					onClick={() => onChange(value)}
				>
					{label}
				</Button>
			))}
		</div>
	);
}

/** Reads the `view` search param: the table is the default and never written to the url. */
export function parseViewMode(value: unknown): ViewMode | undefined {
	return value === "kanban" ? "kanban" : undefined;
}
