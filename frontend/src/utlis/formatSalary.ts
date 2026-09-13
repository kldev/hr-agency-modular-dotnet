export function formatSalary(value: number): string {
	return Math.trunc(value).toLocaleString("pl-PL");
}
