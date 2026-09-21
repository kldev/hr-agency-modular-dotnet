import clsx from "clsx";
import "./tabs.css";

export type TabDefinition<T extends string> = {
	id: T;
	label: string;
	/** Shown as a pill next to the label. Skip it where a count means nothing. */
	count?: number;
};

type TabsProps<T extends string> = {
	value: T;
	tabs: readonly TabDefinition<T>[];
	onChange: (value: T) => void;
	className?: string;
	label?: string;
};

/**
 * A plain tab bar: it renders the strip and reports what was clicked, and the caller decides what
 * that means. Deliberately unaware of routing, so the selected tab can live in the URL, in state,
 * or nowhere at all.
 */
export function Tabs<T extends string>({
	value,
	tabs,
	onChange,
	className,
	label = "Sections",
}: TabsProps<T>) {
	return (
		<div className={clsx("tabs", className)} role="tablist" aria-label={label}>
			{tabs.map((tab) => (
				<button
					key={tab.id}
					type="button"
					role="tab"
					id={`tab-${tab.id}`}
					aria-selected={value === tab.id}
					aria-controls={`tabpanel-${tab.id}`}
					className={clsx("tabs-tab", { "is-selected": value === tab.id })}
					onClick={() => onChange(tab.id)}
				>
					{tab.label}

					{tab.count === undefined ? null : <span className="tabs-count">{tab.count}</span>}
				</button>
			))}
		</div>
	);
}

type TabPanelProps = {
	id: string;
	children: React.ReactNode;
};

/**
 * The panel takes over the spacing of the column it was dropped into: it sits between the layout
 * and the sections, so without a gap of its own everything inside it collapses together.
 */
export function TabPanel({ id, children }: TabPanelProps) {
	return (
		<div role="tabpanel" id={`tabpanel-${id}`} aria-labelledby={`tab-${id}`} className="tabs-panel">
			{children}
		</div>
	);
}
