import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import type { ChangeOpportunityStageRequest } from "#/api/models";
import type { OnSucess } from "#/types";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useChangeStage } from "../opportunity/useOpportunityForm";
import type { ChangeStageRef, OpportunityInfo } from "../SalesCommand";
import { ChangeStageForm } from "./ChangeStageForm";

const ChangeStageDrawer = forwardRef<ChangeStageRef, OnSucess>(({ onSuccess }, ref) => {
	const [isOpen, setIsOpen] = useState(false);
	const [opportunityInfo, setOpportunityInfo] = useState<OpportunityInfo | null>(null);
	const { mutation, waiting, changeStage } = useChangeStage({
		onSuccess: () => {
			onSuccess();

			setOpportunityInfo(null);
			mutation.reset();
			setIsOpen(false);
		},
	});

	useImperativeHandle(
		ref,
		() => ({
			changeStage: (info) => {
				setOpportunityInfo(info);

				mutation.reset();
				setIsOpen(true);
			},
		}),
		[mutation],
	);

	const handleSave = useCallback(
		(value: ChangeOpportunityStageRequest) => {
			changeStage(opportunityInfo?.id ?? "", value.stage, value.lostReason ?? "");
		},
		[changeStage, opportunityInfo],
	);

	const handleClose = useCallback(() => {
		if (mutation.isPending) {
			return;
		}

		setOpportunityInfo(null);

		mutation.reset();
		setIsOpen(false);
	}, [mutation]);

	return (
		<Drawer
			open={isOpen}
			title="Change opportunity stage"
			onClose={handleClose}
			footer={
				<SaveChangesButton form="change-stage" isPending={mutation.isPending} wait={waiting} />
			}
		>
			{opportunityInfo ? (
				<ChangeStageForm
					info={opportunityInfo}
					formId="change-stage"
					onSubmit={handleSave}
					error={mutation.error}
					isSubmitting={mutation.isPending}
				/>
			) : null}
		</Drawer>
	);
});

ChangeStageDrawer.displayName = "ChangeStageDrawer";

export default ChangeStageDrawer;
