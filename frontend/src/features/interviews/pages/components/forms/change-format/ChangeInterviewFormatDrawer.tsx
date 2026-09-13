import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { changeInterviewFormat } from "@/api/endpoints";
import type { ChangeInterviewFormatRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import type { ChangeInterviewStatusCommand } from "../InterviewCommand";

import { ChangeInterviewFormatForm, emptyFormat } from "./ChangeInterviewFormatForm";

interface ChangeInterviewFormatDrawerProps {
	onSuccess: () => void;
}

const ChangeInterviewFormatDrawer = forwardRef<
	ChangeInterviewStatusCommand,
	ChangeInterviewFormatDrawerProps
>(({ onSuccess }, ref) => {
	const [isOpen, setIsOpen] = useState(false);
	const [interviewId, setInterviewId] = useState<string | null>(null);

	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ id, request }: { id: string; request: ChangeInterviewFormatRequest }) =>
			changeInterviewFormat(id, request),

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
		(data: ChangeInterviewFormatRequest) => {
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
			title="Change interview format"
			onClose={handleClose}
			footer={
				<SaveChangesButton
					form="interview-format-from"
					isPending={mutation.isPending}
					wait={waiting}
				/>
			}
		>
			<ChangeInterviewFormatForm
				initialValue={emptyFormat}
				formId="interview-format-from"
				onSubmit={handleSave}
				error={mutation.error}
				isSubmitting={mutation.isPending}
			/>
		</Drawer>
	);
});

ChangeInterviewFormatDrawer.displayName = "ChangeInterviewFormatDrawer";

export default ChangeInterviewFormatDrawer;
