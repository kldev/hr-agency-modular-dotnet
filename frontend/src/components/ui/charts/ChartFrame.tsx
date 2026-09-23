import type { ReactElement } from "react";
import { ResponsiveContainer } from "recharts";

interface ChartFrameProps {
	/** What the chart shows, for assistive tech - an SVG of bars says nothing on its own. */
	label: string;
	height?: number;
	children: ReactElement;
}

/**
 * A chart's box: a fixed height, the full width of its section, and a name. ResponsiveContainer
 * measures its parent, so the height has to come from here or the chart collapses to nothing.
 */
export function ChartFrame({ label, height = 280, children }: ChartFrameProps) {
	return (
		<div role="img" aria-label={label} style={{ width: "100%", height }}>
			<ResponsiveContainer width="100%" height="100%">
				{children}
			</ResponsiveContainer>
		</div>
	);
}
