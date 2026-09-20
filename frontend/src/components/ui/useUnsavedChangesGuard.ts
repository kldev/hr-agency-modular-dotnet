import { useCallback, useState } from "react";

/**
 * A dialog closes on a stray Escape or a click on the cross, where a route does not - so anything
 * already typed into one is worth a question before it disappears. The host keeps its own "which
 * record am I editing" state; this only answers whether closing is safe.
 */
export function useUnsavedChangesGuard(onClose: () => void) {
	const [dirty, setDirty] = useState(false);
	const [confirming, setConfirming] = useState(false);

	const discard = useCallback(() => {
		setConfirming(false);
		setDirty(false);
		onClose();
	}, [onClose]);

	const requestClose = useCallback(() => {
		if (dirty) {
			setConfirming(true);
			return;
		}

		discard();
	}, [dirty, discard]);

	const keepEditing = useCallback(() => {
		setConfirming(false);
	}, []);

	return { setDirty, confirming, requestClose, discard, keepEditing };
}
