import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { changeInterviewStatus } from "@/api/endpoints";
import type { ChangeInterviewStatusRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import type { ChangeInterviewStatusCommand } from "../InterviewCommand";

import { ChangeInterviewStatusForm, empty } from "./ChangeInterviewStatusForm";

interface ChangeInterviewStatusDrawerProps {
	onSuccess: () => void;
}

const ChangeInterviewStatusDrawer = forwardRef<
	ChangeInterviewStatusCommand,
	ChangeInterviewStatusDrawerProps
>(({ onSuccess }, ref) => {
	const [isOpen, setIsOpen] = useState(false);
	const [interviewId, setInterviewId] = useState<string | null>(null);

	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ id, request }: { id: string; request: ChangeInterviewStatusRequest }) =>
			changeInterviewStatus(id, request),

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
			changeStatus: (id: string) => {
				mutation.reset();
				setInterviewId(id);
				setIsOpen(true);
			},
		}),
		[mutation],
	);

	const handleSave = useCallback(
		(data: ChangeInterviewStatusRequest) => {
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
			title="Change interview status"
			onClose={handleClose}
			footer={
				<SaveChangesButton
					form="interview-status-from"
					isPending={mutation.isPending}
					wait={waiting}
				/>
			}
		>
			<ChangeInterviewStatusForm
				initialValue={empty}
				formId="interview-status-from"
				onSubmit={handleSave}
				error={mutation.error}
			/>
		</Drawer>
	);
});

ChangeInterviewStatusDrawer.displayName = "ChangeInterviewStatusDrawer";

export default ChangeInterviewStatusDrawer;
