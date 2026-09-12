export async function copyToClipboard(value: string): Promise<void> {
	if (navigator.clipboard) {
		await navigator.clipboard.writeText(value);
	}
}
