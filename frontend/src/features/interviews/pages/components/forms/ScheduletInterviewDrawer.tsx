import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { scheduleInterview } from "@/api/endpoints";
import type { ScheduleInterviewRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import type { ScheduleInterviewCommand } from "./InterviewCommand";
import { emptyScheduleInterview, InterviewForm } from "./InterviewForm";

interface CreateUserDrawerProps {
	onSuccess: () => void;
}

const ScheduletInterviewDrawer = forwardRef<ScheduleInterviewCommand, CreateUserDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [interview, setInterview] = useState<ScheduleInterviewRequest>(emptyScheduleInterview);
		const [jobApplicationId, setJobApplicationIdtInterview] = useState<string>("");

		const { wait, waiting } = useProjectionWait();

		const createMutation = useMutation({
			mutationFn: ({
				jobApplicationId,
				request,
			}: {
				jobApplicationId: string;
				request: ScheduleInterviewRequest;
			}) => scheduleInterview({ ...request, jobApplicationId: jobApplicationId }),

			onSuccess: async () => {
				await wait();

				setIsOpen(false);
				setInterview(emptyScheduleInterview);

				onSuccess();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				schedule: (jobApplicationId) => {
					createMutation.reset();
					setJobApplicationIdtInterview(jobApplicationId);
					setInterview(emptyScheduleInterview);
					setIsOpen(true);
				},
			}),
			[createMutation],
		);

		const handleSave = useCallback(
			(value: ScheduleInterviewRequest) => {
				createMutation.mutate({ jobApplicationId: jobApplicationId, request: value });
			},
			[createMutation],
		);

		const handleClose = useCallback(() => {
			if (createMutation.isPending) {
				return;
			}

			createMutation.reset();
			setIsOpen(false);
		}, [createMutation]);

		return (
			<Drawer
				open={isOpen}
				title="Create user"
				onClose={handleClose}
				footer={
					<SaveChangesButton
						form="interview-form"
						isPending={createMutation.isPending}
						wait={waiting}
					/>
				}
			>
				<InterviewForm
					initialValue={interview}
					onSubmit={handleSave}
					error={createMutation.error}
					isSubmitting={createMutation.isPending}
				/>
			</Drawer>
		);
	},
);

ScheduletInterviewDrawer.displayName = "ScheduletInterviewDrawer";

export default ScheduletInterviewDrawer;
