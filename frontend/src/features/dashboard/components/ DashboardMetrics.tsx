import { BriefcaseBusiness, CalendarDays, CheckCircle2, FileText, Users } from "lucide-react";
import { MetricCard, Metrics } from "@/components/ui/MetricCard";

export function DashboardMetrics() {
	return (
		<Metrics>
			<MetricCard label="Open positions" value={24} icon={<BriefcaseBusiness />} />

			<MetricCard label="Active candidates" value={186} icon={<Users />} />

			<MetricCard label="New applications" value={38} icon={<FileText />} />

			<MetricCard label="Interviews today" value={4} icon={<CalendarDays />} />

			<MetricCard label="Hired" value={120} icon={<CheckCircle2 />} />
		</Metrics>
	);
}
