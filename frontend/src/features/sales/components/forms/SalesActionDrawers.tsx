import { forwardRef, useImperativeHandle, useRef } from "react";
import type { OnSucess } from "#/types";
import { LogActivityDrawer } from "./activity";
import { ChangeStageDrawer } from "./change-stage";
import { FollowUpDrawer } from "./follow-up";
import { EditOpportunityDrawer } from "./opportunity";
import type {
	ChangeStageRef,
	EditOpportunityRef,
	FollowUpRef,
	LogActionRef,
	SalesActionRef,
	SalesActionTypes,
} from "./SalesCommand";

const SalesActionDrawers = forwardRef<SalesActionRef, OnSucess>(({ onSuccess }, ref) => {
	const editRef = useRef<EditOpportunityRef>(null);
	const logAction = useRef<LogActionRef>(null);
	const changeStage = useRef<ChangeStageRef>(null);
	const followUp = useRef<FollowUpRef>(null);

	useImperativeHandle(
		ref,
		() => ({
			changeStage: (info) => {
				changeStage.current?.changeStage(info);
			},
			addFollowUp: (opportunityId) => {
				followUp.current?.add(opportunityId);
			},
			editFollowUp: (info) => {
				followUp.current?.edit(info);
			},
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

	return (
		<>
			<EditOpportunityDrawer ref={editRef} onSuccess={onSuccess} />
			<LogActivityDrawer ref={logAction} onSuccess={onSuccess} />
			<ChangeStageDrawer ref={changeStage} onSuccess={onSuccess} />
			<FollowUpDrawer ref={followUp} onSuccess={onSuccess} />
		</>
	);
});

SalesActionDrawers.displayName = "SalesActionDrawers";

export default SalesActionDrawers;
