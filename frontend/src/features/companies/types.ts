import { Industry } from "@/api/models";

export const industries = Object.fromEntries(
	Object.values(Industry).map((value) => [value, value]),
) as Record<Industry, string>;
