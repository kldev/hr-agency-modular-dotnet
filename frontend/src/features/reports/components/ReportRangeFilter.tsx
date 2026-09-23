import { EnumFilter } from "@/components/ui/EnumFilter";
import { type ReportRange, reportRanges } from "../period";

interface ReportRangeFilterProps {
	value: ReportRange;
	onChange: (value: ReportRange) => void;
}

export function ReportRangeFilter({ value, onChange }: ReportRangeFilterProps) {
	return (
		<EnumFilter
			hideAll
			value={value}
			options={reportRanges}
			onChange={(range) => {
				if (range) onChange(range);
			}}
		/>
	);
}
