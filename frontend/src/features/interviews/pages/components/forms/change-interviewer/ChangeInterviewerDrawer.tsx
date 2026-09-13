import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { changeInterviewer } from "@/api/endpoints";
import type { ChangeInterviewerRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import type { ChangeInterviewerCommand } from "../InterviewCommand";

import { ChangeInterviewerForm, emptyForm } from "./ChangeInterviewerForm";

interface ChangeInterviewerDrawerProps {
	onSuccess: () => void;
}

const ChangeInterviewerDrawer = forwardRef<ChangeInterviewerCommand, ChangeInterviewerDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [interviewId, setInterviewId] = useState<string | null>(null);

		const { wait, waiting } = useProjectionWait();

		const mutation = useMutation({
			mutationFn: ({ id, request }: { id: string; request: ChangeInterviewerRequest }) =>
				changeInterviewer(id, request),

			onSuccess: async () => {
				await wait();

				setIsOpen(false);
				setInterviewId(null);

				onSuccess();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				changeInterviewer: (id: string) => {
					mutation.reset();
					setInterviewId(id);
					setIsOpen(true);
				},
			}),
			[mutation],
		);

		const handleSave = useCallback(
			(data: ChangeInterviewerRequest) => {
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
				<ChangeInterviewerForm
					initialValue={emptyForm}
					formId="change-interviewer-from"
					onSubmit={handleSave}
					error={mutation.error}
					isSubmitting={mutation.isPending}
				/>
			</Drawer>
		);
	},
);

ChangeInterviewerDrawer.displayName = "ChangeInterviewerDrawer";

export default ChangeInterviewerDrawer;
