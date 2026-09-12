import {
	createSortedRowModel,
	metaHelper,
	rowSortingFeature,
	tableFeatures,
} from "@tanstack/react-table";

export type CellWidth = "xxs" | "xs" | "sm" | "md" | "xl" | "2xl";

export interface AppColumnMeta {
	className?: string;
	align?: "left" | "center" | "right";
	width?: CellWidth;
}

export const appTableFeatures = tableFeatures({
	rowSortingFeature,
	sortedRowModel: createSortedRowModel(),

	sortFns: {
		text: (rowA, rowB, columnId) => {
			const a = String(rowA.getValue(columnId) ?? "");
			const b = String(rowB.getValue(columnId) ?? "");

			return a.localeCompare(b, undefined, {
				sensitivity: "base",
			});
		},
	},
	columnMeta: metaHelper<AppColumnMeta>(),
});

export type appTableFeaturesType = typeof appTableFeatures;
