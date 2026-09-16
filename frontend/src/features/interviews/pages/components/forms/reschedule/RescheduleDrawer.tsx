import { format } from "date-fns";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { useGetInterview } from "#/features/calendar/pages/hooks/useCalendar";
import type { RescheduleInterviewRequest } from "@/api/models";
import { DetailOverviewHeader, SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import type { RescheduleInterviewCommand } from "../InterviewCommand";
import { RescheduleForm } from "./RescheduleForm";
import { useRescheduleInterview } from "./useRescheduleInterview";

interface RescheduleDrawerProps {
	onSuccess: () => void;
}

const RescheduleDrawer = forwardRef<RescheduleInterviewCommand, RescheduleDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [interviewId, setInterviewId] = useState<string | null>(null);

		const interviewQuery = useGetInterview(interviewId ?? "");

		const { waiting, schedule, error, isPending } = useRescheduleInterview({
			onSuccess: () => {
				setIsOpen(false);
				setInterviewId(null);
				onSuccess();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				reschedule: (id: string) => {
					setInterviewId(id);
					setIsOpen(true);
				},
			}),
			[],
		);

		const handleSave = useCallback(
			(data: RescheduleInterviewRequest) => {
				if (!interviewId) {
					return;
				}

				schedule(interviewId, data);
			},
			[interviewId, schedule],
		);

		const handleClose = useCallback(() => {
			if (isPending) {
				return;
			}

			setIsOpen(false);
			setInterviewId(null);
		}, [isPending]);

		const interview = interviewQuery.data;

		return (
			<Drawer
				open={isOpen}
				title="Reschedule interview"
				onClose={handleClose}
				footer={
					<SaveChangesButton form="change-interviewer-from" isPending={isPending} wait={waiting} />
				}
			>
				{interviewQuery.isLoading && <div className="form-loading">Loading application...</div>}

				{interviewQuery.isError && (
					<div className="form-error" role="alert">
						Unable to load application.
					</div>
				)}
				{interview ? (
					<>
						<DetailOverviewHeader
							className=" mb-4"
							title={interview.applicantInfo.fullName ?? ""}
							description={interview.applicantInfo.email}
						></DetailOverviewHeader>

						<RescheduleForm
							initialValue={{
								note: "",
								scheduledAt: toLocalDateTimeInput(interview.scheduleAt),
								scheduledTimezone: interview.timezone,
								location: interview.location ?? "",
								meetingUrl: interview.meetingUrl ?? "",
							}}
							formId="change-interviewer-from"
							onSubmit={handleSave}
							error={error}
							isSubmitting={isPending}
						/>
					</>
				) : null}
			</Drawer>
		);
	},
);

const toLocalDateTimeInput = (utc: string) => format(new Date(utc), "yyyy-MM-dd'T'HH:mm");

RescheduleDrawer.displayName = "RescheduleDrawer";

export default RescheduleDrawer;
