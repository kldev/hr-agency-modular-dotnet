/**
 * Simulates network latency.
 */
export function delay(ms: number, signal: AbortSignal): Promise<void> {
	return new Promise((resolve, reject) => {
		if (signal.aborted) {
			reject(new DOMException("Request aborted", "AbortError"));

			return;
		}

		const timeoutId = window.setTimeout(() => {
			signal.removeEventListener("abort", handleAbort);

			resolve();
		}, ms);

		const handleAbort = () => {
			window.clearTimeout(timeoutId);

			reject(new DOMException("Request aborted", "AbortError"));
		};

		signal.addEventListener("abort", handleAbort, {
			once: true,
		});
	});
}
