import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { format } from "date-fns";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { getInterview, rescheduleInterview } from "@/api/endpoints";
import type { RescheduleInterviewRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import type { RescheduleInterviewCommand } from "../InterviewCommand";
import { RescheduleForm } from "./RescheduleForm";

interface RescheduleDrawerProps {
	onSuccess: () => void;
}

const RescheduleDrawer = forwardRef<RescheduleInterviewCommand, RescheduleDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [interviewId, setInterviewId] = useState<string | null>(null);

		const { wait, waiting } = useProjectionWait();

		const queryClient = useQueryClient();

		const interviewQuery = useQuery({
			queryKey: ["interview", interviewId],
			queryFn: () => getInterview(interviewId as string),
			enabled: isOpen && interviewId != null && interviewId.length > 0,
		});

		const mutation = useMutation({
			mutationFn: ({ id, request }: { id: string; request: RescheduleInterviewRequest }) =>
				rescheduleInterview(id, request),

			onSuccess: async () => {
				await wait();

				queryClient.invalidateQueries({
					queryKey: ["interview", interviewId],
				});

				setIsOpen(false);
				setInterviewId(null);

				onSuccess();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				reschedule: (id: string) => {
					queryClient.invalidateQueries({
						queryKey: ["interview", id],
					});

					mutation.reset();
					setInterviewId(id);
					setIsOpen(true);
				},
			}),
			[mutation],
		);

		const handleSave = useCallback(
			(data: RescheduleInterviewRequest) => {
				if (!interviewId) {
					return;
				}

				mutation.mutate({
					id: interviewId,
					request: data,
				});
			},
			[interviewId, mutation],
		);

		const handleClose = useCallback(() => {
			if (mutation.isPending) {
				return;
			}

			mutation.reset();
			setIsOpen(false);
			setInterviewId(null);
		}, [mutation]);

		const interview = interviewQuery.data;

		return (
			<Drawer
				open={isOpen}
				title="Change interviewer"
				onClose={handleClose}
				footer={
					<SaveChangesButton
						form="change-interviewer-from"
						isPending={mutation.isPending}
						wait={waiting}
					/>
				}
			>
				{interviewQuery.isLoading && <div className="form-loading">Loading application...</div>}

				{interviewQuery.isError && (
					<div className="form-error" role="alert">
						Unable to load application.
					</div>
				)}
				{interview ? (
					<RescheduleForm
						initialValue={{
							note: "",
							scheduledAt: toLocalDateTimeInput(interview.scheduleAt),
							scheduledTimezone: interview.timezone,
						}}
						formId="change-interviewer-from"
						onSubmit={handleSave}
						error={mutation.error}
						isSubmitting={mutation.isPending}
					/>
				) : null}
			</Drawer>
		);
	},
);

const toLocalDateTimeInput = (utc: string) => format(new Date(utc), "yyyy-MM-dd'T'HH:mm");

RescheduleDrawer.displayName = "RescheduleDrawer";

export default RescheduleDrawer;
