import { forwardRef, useImperativeHandle, useRef } from "react";

import { EditOpportunityDrawer } from "./opportunity";
import type {
	EditOpportunityRef,
	LogActionRef,
	SalesActionRef,
	SalesActionTypes,
} from "./SalesCommand";

interface SalesActionDrawersProps {
	onSuccess: () => void;
}

const SalesActionDrawers = forwardRef<SalesActionRef, SalesActionDrawersProps>(
	({ onSuccess }, ref) => {
		const editRef = useRef<EditOpportunityRef>(null);
		const logAction = useRef<LogActionRef>(null);

		useImperativeHandle(
			ref,
			() => ({
				onAction: (id: string, action: SalesActionTypes) => {
					switch (action) {
						case "edit-opportunity":
							editRef.current?.edit(id);
							break;
						case "log-activity":
							logAction.current?.log(id);
							break;
					}
				},
			}),
			[],
		);

		return <EditOpportunityDrawer ref={editRef} onSuccess={onSuccess} />;
	},
);

SalesActionDrawers.displayName = "SalesActionDrawers";

export default SalesActionDrawers;
