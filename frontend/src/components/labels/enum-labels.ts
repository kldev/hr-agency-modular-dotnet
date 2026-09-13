import type { EmploymentType, JobDescriptionStatus, JobPostStatus, WorkMode } from "@/api/models";

export function getEmploymentTypeLabel(type: EmploymentType) {
	switch (type) {
		case "FullTime":
			return "Full time";
		case "PartTime":
			return "Part time";
		case "Contract":
			return "Contract";
		case "Temporary":
			return "Temporary";
		case "Internship":
			return "Internship";
		default:
			return type;
	}
}

export function getWorkModeLabel(mode: WorkMode) {
	switch (mode) {
		case "Remote":
			return "Remote";
		case "Hybrid":
			return "Hybrid";
		case "OnSite":
			return "On-site";
		default:
			return mode;
	}
}

export function getCountryLabel(countryCode: string) {
	try {
		return new Intl.DisplayNames(["en"], {
			type: "region",
		}).of(countryCode);
	} catch {
		return countryCode;
	}
}

export function getJobDescriptionStatusLabel(status: JobDescriptionStatus) {
	switch (status) {
		case "Draft":
			return "Draft";
		case "Open":
			return "Open";
		case "OnHold":
			return "On hold";
		case "Closed":
			return "Closed";
		case "Cancelled":
			return "Cancelled";
		default:
			return status;
	}
}

export function getJobPostStatusLabel(status: JobPostStatus) {
	switch (status) {
		case "Draft":
			return "Draft";
		case "Published":
			return "Open";
		case "Archived":
			return "On hold";

		default:
			return status;
	}
}

export function formatSalary(min: number | string, max: number | string, currencyCode: string) {
	const formatter = new Intl.NumberFormat("pl-PL", {
		minimumFractionDigits: 0,
		maximumFractionDigits: 2,
	});

	return `${formatter.format(Number(min))} – ${formatter.format(Number(max))} ${currencyCode}`;
}
