import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { changeJobPostStatus } from "@/api/endpoints";
import type { ChangeJobPostStatusRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import type { ChangeJobPostStatusCommand } from "../JobPostsCommand";

import { ChangeJobPostStatusForm, emptyChangeJobPostStatus } from "./ChangeJobPostStatusForm";

interface ChangeJobPostStatusDrawerProps {
	onSuccess: () => void;
}

const ChangeJobPostStatusDrawer = forwardRef<
	ChangeJobPostStatusCommand,
	ChangeJobPostStatusDrawerProps
>(({ onSuccess }, ref) => {
	const [isOpen, setIsOpen] = useState(false);
	const [jobPostId, setJobPostId] = useState<string | null>(null);

	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ id, request }: { id: string; request: ChangeJobPostStatusRequest }) =>
			changeJobPostStatus(id, request),

		onSuccess: async () => {
			await wait();

			setIsOpen(false);
			setJobPostId(null);

			onSuccess();
		},
	});

	useImperativeHandle(
		ref,
		() => ({
			changeStatus: (id: string) => {
				mutation.reset();
				setJobPostId(id);
				setIsOpen(true);
			},
		}),
		[mutation],
	);

	const handleSave = useCallback(
		(data: ChangeJobPostStatusRequest) => {
			if (!jobPostId) {
				return;
			}

			mutation.mutate({
				id: jobPostId,
				request: data,
			});
		},
		[jobPostId, mutation],
	);

	const handleClose = useCallback(() => {
		if (mutation.isPending) {
			return;
		}

		mutation.reset();
		setIsOpen(false);
		setJobPostId(null);
	}, [mutation]);

	return (
		<Drawer
			open={isOpen}
			title="Change job post status"
			onClose={handleClose}
			footer={
				<SaveChangesButton
					form="change-job-post-status-form"
					isPending={mutation.isPending}
					wait={waiting}
				/>
			}
		>
			<ChangeJobPostStatusForm
				initialValue={emptyChangeJobPostStatus}
				onSubmit={handleSave}
				error={mutation.error}
			/>
		</Drawer>
	);
});

ChangeJobPostStatusDrawer.displayName = "ChangeJobPostStatusDrawer";

export default ChangeJobPostStatusDrawer;
