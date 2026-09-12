import { useCallback, useState } from "react";

export function useProjectionWait(delay = 1500) {
	const [waiting, setWaiting] = useState(false);

	const wait = useCallback(async () => {
		setWaiting(true);

		await new Promise((resolve) => {
			setTimeout(resolve, delay);
		});

		setWaiting(false);
	}, [delay]);

	return {
		waiting,
		wait,
	};
}
