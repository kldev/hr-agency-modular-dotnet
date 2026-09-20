import { useEffect } from "react";

/**
 * Whether the fields of the current step have anything wrong with them.
 *
 * A schema issue can land on a nested path (`responsibilities[0]`), which the array field itself
 * does not see - hence the prefix match rather than an equality check.
 */
export function stepHasErrors(
	fieldMeta: Record<string, { errors: Array<unknown> }>,
	fields: readonly string[],
) {
	return Object.entries(fieldMeta).some(
		([name, meta]) =>
			meta.errors.length > 0 &&
			fields.some(
				(field) => name === field || name.startsWith(`${field}[`) || name.startsWith(`${field}.`),
			),
	);
}

/** Lets a wizard hosted in a dialog tell its host whether closing would throw anything away. */
export function DirtyReporter({
	isDirty,
	onDirtyChange,
}: {
	isDirty: boolean;
	onDirtyChange: (dirty: boolean) => void;
}) {
	useEffect(() => {
		onDirtyChange(isDirty);
	}, [isDirty, onDirtyChange]);

	return null;
}
