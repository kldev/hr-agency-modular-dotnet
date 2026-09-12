import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { changeJobApplicationStatus } from "@/api/endpoints";
import type {
	ChangeJobApplicationStatusRequest,
	JobApplicationStatus,
	JobApplicationUpdateStatus,
} from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";

import {
	ChangeJobApplicationStatusForm,
	emptyChangeJobApplicationStatus,
} from "./ChangeJobApplicationStatusForm";

export interface ChangeJobApplicationStatusFormCommand {
	changeStatus(jobApplicationId: string, curent: JobApplicationStatus): void;
}

interface ChangeJobApplicationStatusDrawerProps {
	onSuccess: () => void;
}

const getNextStatus = (status: JobApplicationStatus): JobApplicationUpdateStatus => {
	switch (status) {
		case "Applied":
			return "Screening";
		case "Screening":
			return "Interview";
		case "Interview":
			return "Assessment";
		case "Assessment":
			return "Offer";
	}
	return status;
};

const ChangeJobApplicationStatusDrawer = forwardRef<
	ChangeJobApplicationStatusFormCommand,
	ChangeJobApplicationStatusDrawerProps
>(({ onSuccess }, ref) => {
	const [isOpen, setIsOpen] = useState(false);
	const [jobApplicationId, setJobApplicationId] = useState<string | null>(null);
	const [value, setValue] = useState<ChangeJobApplicationStatusRequest>(
		emptyChangeJobApplicationStatus,
	);

	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ id, request }: { id: string; request: ChangeJobApplicationStatusRequest }) =>
			changeJobApplicationStatus(id, request),

		onSuccess: async () => {
			await wait();

			setIsOpen(false);
			setJobApplicationId(null);
			setValue(emptyChangeJobApplicationStatus);

			onSuccess();
		},
	});

	useImperativeHandle(
		ref,
		() => ({
			changeStatus: (id: string, status: JobApplicationStatus) => {
				mutation.reset();
				setJobApplicationId(id);
				setValue({ ...emptyChangeJobApplicationStatus, status: getNextStatus(status) });
				setIsOpen(true);
			},
		}),
		[mutation],
	);

	const handleSave = useCallback(
		(data: ChangeJobApplicationStatusRequest) => {
			if (!jobApplicationId) {
				return;
			}

			mutation.mutate({
				id: jobApplicationId,
				request: data,
			});
		},
		[jobApplicationId, mutation],
	);

	const handleClose = useCallback(() => {
		if (mutation.isPending) {
			return;
		}

		mutation.reset();
		setIsOpen(false);
		setJobApplicationId(null);
		setValue(emptyChangeJobApplicationStatus);
	}, [mutation]);

	return (
		<Drawer
			open={isOpen}
			title="Change application status"
			onClose={handleClose}
			footer={
				<SaveChangesButton
					form="change-job-application-status-form"
					isPending={mutation.isPending}
					wait={waiting}
				/>
			}
		>
			<ChangeJobApplicationStatusForm
				initialValue={value}
				onSubmit={handleSave}
				error={mutation.error}
				isSubmitting={mutation.isPending}
			/>
		</Drawer>
	);
});

ChangeJobApplicationStatusDrawer.displayName = "ChangeJobApplicationStatusDrawer";

export default ChangeJobApplicationStatusDrawer;
