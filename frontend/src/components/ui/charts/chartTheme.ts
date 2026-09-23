/**
 * The chart palette, taken from the theme tokens so a chart follows the light and dark theme like
 * the rest of the page. SVG presentation attributes accept `var(...)`, so a feature passes these
 * straight to recharts and never spells a colour itself.
 */
export const chartColors = {
	primary: "var(--color-primary)",
	success: "var(--color-success)",
	warning: "var(--color-warning)",
	info: "var(--color-info)",
	violet: "var(--color-violet)",
	cyan: "var(--color-cyan)",
	orange: "var(--color-orange)",
	danger: "var(--color-danger)",
	muted: "var(--color-text-muted)",
	grid: "var(--color-border-subtle)",
} as const;

/** Axis ticks in the muted text colour and the small size the tables use. */
export const axisTick = { fill: chartColors.muted, fontSize: 12 } as const;

/** The tooltip as a surface card rather than recharts' white box, which glares in the dark theme. */
export const tooltipStyle = {
	contentStyle: {
		background: "var(--color-surface)",
		border: "1px solid var(--color-border)",
		borderRadius: 8,
		fontSize: 12,
		color: "var(--color-text)",
	},
	labelStyle: { color: "var(--color-text)", fontWeight: 600 },
	itemStyle: { color: "var(--color-text-secondary)" },
	cursor: { fill: "var(--color-surface-hover)" },
} as const;
