import { forwardRef, useImperativeHandle, useRef } from "react";
import type { JobApplicationStatus } from "#/api/models";
import {
	type ScheduleInterviewCommand,
	ScheduletInterviewDrawer,
} from "@/features/interviews/pages/components";
import {
	AddJobApplicationNoteDrawer,
	type AddJobApplicationNoteFormCommand,
	ChangeJobApplicationStatusDrawer,
	type ChangeJobApplicationStatusFormCommand,
	type EditApplicantCommand,
	EditApplicantDrawer,
} from "../forms";
import type { JobApplicationsActionsType, JobApplicationsRef } from ".";
import { type AddTagCommand, AddTagsDrawer } from "./add-tag";

interface ApplicationsActionDrawersProps {
	onSuccess: () => void;
}

const ApplicationsActionDrawers = forwardRef<JobApplicationsRef, ApplicationsActionDrawersProps>(
	({ onSuccess }, ref) => {
		const changeStatusRef = useRef<ChangeJobApplicationStatusFormCommand>(null);
		const addNoteRef = useRef<AddJobApplicationNoteFormCommand>(null);
		const editRef = useRef<EditApplicantCommand>(null);
		const scheduleRef = useRef<ScheduleInterviewCommand>(null);
		const tagRef = useRef<AddTagCommand>(null);

		useImperativeHandle(
			ref,
			() => ({
				update: (
					id: string,
					action: JobApplicationsActionsType,
					current?: JobApplicationStatus,
				) => {
					switch (action) {
						case "add-note":
							addNoteRef.current?.addNote(id);
							break;
						case "edit":
							editRef.current?.edit(id);
							break;
						case "change-status":
							changeStatusRef.current?.changeStatus(id, current ?? "Applied");
							break;
						case "schedule":
							scheduleRef.current?.schedule(id);
							break;
						case "tag":
							tagRef.current?.addTag(id, "", "application");
					}
				},
			}),
			[],
		);

		return (
			<>
				<AddJobApplicationNoteDrawer ref={addNoteRef} onSuccess={onSuccess} />
				<ChangeJobApplicationStatusDrawer ref={changeStatusRef} onSuccess={onSuccess} />
				<EditApplicantDrawer ref={editRef} onSuccess={onSuccess} />
				<ScheduletInterviewDrawer ref={scheduleRef} onSuccess={onSuccess} />
				<AddTagsDrawer ref={tagRef} onSuccess={onSuccess} />
			</>
		);
	},
);

ApplicationsActionDrawers.displayName = "ApplicationsActionDrawers";

export default ApplicationsActionDrawers;
