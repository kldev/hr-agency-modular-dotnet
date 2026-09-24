import type { ReactNode } from "react";
import { EmptyState } from "#/components/ui";

interface TabStateProps {
	isLoading: boolean;
	isError: boolean;
	isEmpty: boolean;
	what: string;
	emptyIcon: ReactNode;
	emptyDescription: string;
	children: ReactNode;
}

/** Loading, failed, empty or the content itself - the same four answers on every tab. */
export function TabState({
	isLoading,
	isError,
	isEmpty,
	what,
	emptyIcon,
	emptyDescription,
	children,
}: TabStateProps) {
	if (isLoading) return <div className="data-details-loading">Loading ...</div>;

	if (isError)
		return (
			<div className="form-error" role="alert">
				{what} could not be loaded.
			</div>
		);

	if (isEmpty)
		return (
			<EmptyState title={`No ${what.toLowerCase()} yet`} description={emptyDescription}>
				{emptyIcon}
			</EmptyState>
		);

	return <>{children}</>;
}
