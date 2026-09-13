import { forwardRef, useImperativeHandle, useRef } from "react";
import { ChangeInterviewFormatDrawer } from "./change-format";
import { ChangeInterviewerDrawer } from "./change-interviewer";
import { ChangeInterviewStatusDrawer } from "./change-status";
import type {
	ChangeInterviewerCommand,
	ChangeInterviewFormatCommand,
	ChangeInterviewStatusCommand,
	InterviewActionsRef,
	InterviewActionsType,
	RescheduleInterviewCommand,
} from "./InterviewCommand";
import { RescheduleDrawer } from "./reschedule";

interface InterviewActionDrawersProps {
	onSuccess: () => void;
}

const InterviewActionDrawers = forwardRef<InterviewActionsRef, InterviewActionDrawersProps>(
	({ onSuccess }, ref) => {
		const interviewerRef = useRef<ChangeInterviewerCommand>(null);
		const statusRef = useRef<ChangeInterviewStatusCommand>(null);

		const formatRef = useRef<ChangeInterviewFormatCommand>(null);
		const rescheduleRef = useRef<RescheduleInterviewCommand>(null);

		useImperativeHandle(
			ref,
			() => ({
				update: (id: string, action: InterviewActionsType) => {
					switch (action) {
						case "change-Interviewer":
							interviewerRef.current?.changeInterviewer(id);
							break;
						case "change-format":
							formatRef.current?.changeFormat(id);
							break;
						case "change-status":
							statusRef.current?.changeStatus(id);
							break;
						case "reschedule":
							rescheduleRef.current?.reschedule(id);
							break;
					}
				},
			}),
			[],
		);

		return (
			<>
				<ChangeInterviewFormatDrawer ref={formatRef} onSuccess={onSuccess} />
				<ChangeInterviewerDrawer ref={interviewerRef} onSuccess={onSuccess} />
				<ChangeInterviewStatusDrawer ref={statusRef} onSuccess={onSuccess} />
				<RescheduleDrawer ref={rescheduleRef} onSuccess={onSuccess} />
			</>
		);
	},
);

InterviewActionDrawers.displayName = "InterviewActionDrawers";

export default InterviewActionDrawers;
