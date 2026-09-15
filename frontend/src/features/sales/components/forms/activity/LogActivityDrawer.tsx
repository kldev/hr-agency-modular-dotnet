import { forwardRef, useCallback, useImperativeHandle, useState } from "react";

import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useLogActivity } from "../opportunity/useOpportunityForm";
import type { LogActionRef } from "../SalesCommand";
import { ActivityForm, type ActivityLogFormValues } from "./ActivityForm";

interface LogActivityDrawerProps {
	onSuccess: () => void;
}

const LogActivityDrawer = forwardRef<LogActionRef, LogActivityDrawerProps>(({ onSuccess }, ref) => {
	const [isOpen, setIsOpen] = useState(false);
	const [opportunityId, setOpportunityId] = useState<string>("");

	const { mutation, waiting } = useLogActivity({
		onSuccess: () => {
			onSuccess();

			setOpportunityId("");
			mutation.reset();
			setIsOpen(false);
		},
	});

	useImperativeHandle(
		ref,
		() => ({
			log: (id: string) => {
				setOpportunityId(id);

				mutation.reset();
				setIsOpen(true);
			},
		}),
		[mutation],
	);

	const handleSave = useCallback(
		(value: ActivityLogFormValues) => {
			mutation.mutate({ request: { opportunityId: opportunityId, ...value } });
		},
		[mutation, opportunityId],
	);

	const handleClose = useCallback(() => {
		if (mutation.isPending) {
			return;
		}

		setOpportunityId("");

		mutation.reset();
		setIsOpen(false);
	}, [mutation]);

	return (
		<Drawer
			open={isOpen}
			title="Create opportunity"
			onClose={handleClose}
			footer={
				<SaveChangesButton form="log-activity" isPending={mutation.isPending} wait={waiting} />
			}
		>
			<ActivityForm
				formId="log-activity"
				onSubmit={handleSave}
				error={mutation.error}
				isSubmitting={mutation.isPending}
			/>
		</Drawer>
	);
});

LogActivityDrawer.displayName = "LogActivityDrawer";

export default LogActivityDrawer;
