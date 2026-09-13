import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { applyToJobPost } from "@/api/endpoints";
import type { ApplyToPostRequest } from "@/api/models";
import { DetailOverviewHeader, SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import type { CreateJobApplicationsCommand } from "../ApplicationsCommand";
import { emptyJobApplications, JobApplicationsForm } from "./JobApplicationsForm";

interface CreateJobApplictionsDrawerProps {
	onSuccess: () => void;
}

const CreateJobApplicationsDrawer = forwardRef<
	CreateJobApplicationsCommand,
	CreateJobApplictionsDrawerProps
>(({ onSuccess }, ref) => {
	const [isOpen, setIsOpen] = useState(false);
	const [jobPostId, setJobPostId] = useState<string>("");
	const [jobPostTitle, setJobPostTitle] = useState<string>("");

	const { wait, waiting } = useProjectionWait();

	const updateMutation = useMutation({
		mutationFn: ({ id, request }: { id: string; request: ApplyToPostRequest }) =>
			applyToJobPost(id, request),

		onSuccess: async () => {
			await wait();

			setIsOpen(false);
			setJobPostId("");
			setJobPostTitle("");
			onSuccess();
		},
	});

	useImperativeHandle(
		ref,
		() => ({
			create: (postId: string, postTitle: string) => {
				setJobPostId(postId);
				setJobPostTitle(postTitle);

				updateMutation.reset();
				setIsOpen(true);
			},
		}),
		[updateMutation],
	);

	const handleSave = useCallback(
		(value: ApplyToPostRequest) => {
			updateMutation.mutate({ id: jobPostId, request: value });
		},
		[updateMutation],
	);

	const handleClose = useCallback(() => {
		if (updateMutation.isPending) {
			return;
		}

		setJobPostId("");
		setJobPostTitle("");
		updateMutation.reset();
		setIsOpen(false);
	}, [updateMutation]);

	return (
		<Drawer
			open={isOpen}
			title="Edit applicant"
			onClose={handleClose}
			footer={
				<SaveChangesButton
					form="job-applications-form"
					isPending={updateMutation.isPending}
					wait={waiting}
				/>
			}
		>
			<DetailOverviewHeader title={jobPostTitle} description="Create applications for job post" />
			<JobApplicationsForm
				initialValue={emptyJobApplications}
				onSubmit={handleSave}
				error={updateMutation.error}
				isSubmitting={updateMutation.isPending}
			/>
		</Drawer>
	);
});

CreateJobApplicationsDrawer.displayName = "CreateJobApplicationsDrawer";

export default CreateJobApplicationsDrawer;
