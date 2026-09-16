import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import type { PersonInfo } from "#/types";
import { scheduleInterview } from "@/api/endpoints";
import type { ScheduleInterviewRequest } from "@/api/models";
import { DetailOverviewHeader, SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import type { ScheduleInterviewCommand } from "../InterviewCommand";
import { InterviewForm } from "./InterviewForm";

interface CreateUserDrawerProps {
	onSuccess: () => void;
}

const ScheduletInterviewDrawer = forwardRef<ScheduleInterviewCommand, CreateUserDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);

		const [jobApplicationId, setJobApplicationIdtInterview] = useState<string>("");
		const [info, setInfo] = useState<PersonInfo | null>(null);

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
				setInfo(null);

				onSuccess();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				schedule: (jobApplicationId, applicant) => {
					createMutation.reset();
					setJobApplicationIdtInterview(jobApplicationId);
					setInfo(applicant);
					setIsOpen(true);
				},
			}),
			[createMutation],
		);

		const handleSave = useCallback(
			(value: ScheduleInterviewRequest) => {
				createMutation.mutate({ jobApplicationId: jobApplicationId, request: value });
			},
			[createMutation, jobApplicationId],
		);

		const handleClose = useCallback(() => {
			if (createMutation.isPending) {
				return;
			}

			createMutation.reset();
			setIsOpen(false);
			setInfo(null);
		}, [createMutation]);

		return (
			<Drawer
				open={isOpen}
				title="Schedule interview"
				onClose={handleClose}
				footer={
					<SaveChangesButton
						form="interview-form"
						isPending={createMutation.isPending}
						wait={waiting}
					/>
				}
			>
				<DetailOverviewHeader
					className=" mb-4"
					title={info?.fullName ?? ""}
					description={info?.email ?? ""}
				></DetailOverviewHeader>

				<InterviewForm
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
