import { Tabs as HeroTabs } from "@heroui/react";
import clsx from "clsx";
import type { Key, ReactNode } from "react";
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
	/** The `TabPanel`s - React Aria ties a panel to its tab only when it sits inside the tabs. */
	children?: ReactNode;
};

/**
 * HeroUI's tabs over a list of definitions: the caller decides what a selection means, so the
 * selected tab can live in the URL, in state, or nowhere at all. Only the selected panel is
 * mounted, which is why pages may render their panels conditionally or as one panel keyed by the
 * active id.
 */
export function Tabs<T extends string>({
	value,
	tabs,
	onChange,
	className,
	label = "Sections",
	children,
}: TabsProps<T>) {
	return (
		<HeroTabs
			className={clsx("panel-tabs", className)}
			selectedKey={value}
			onSelectionChange={(key: Key) => onChange(String(key) as T)}
		>
			<HeroTabs.ListContainer>
				<HeroTabs.List aria-label={label}>
					{tabs.map((tab) => (
						<HeroTabs.Tab key={tab.id} id={tab.id}>
							{tab.label}

							{tab.count === undefined ? null : <span className="tabs-count">{tab.count}</span>}
						</HeroTabs.Tab>
					))}
				</HeroTabs.List>
			</HeroTabs.ListContainer>

			{children}
		</HeroTabs>
	);
}

type TabPanelProps = {
	id: string;
	children: ReactNode;
};

/**
 * The panel takes over the spacing of the column it was dropped into: it sits between the layout
 * and the sections, so without a gap of its own everything inside it collapses together.
 */
export function TabPanel({ id, children }: TabPanelProps) {
	return (
		<HeroTabs.Panel id={id} className="tabs-panel">
			{children}
		</HeroTabs.Panel>
	);
}
