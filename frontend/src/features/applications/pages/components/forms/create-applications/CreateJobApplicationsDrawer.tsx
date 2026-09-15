import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { DetailOverviewHeader, SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";

import type { CreateJobApplicationsCommand } from "../ApplicationsCommand";
import { emptyJobApplications, JobApplicationsForm } from "./JobApplicationsForm";
import { useApplyToJobPost } from "./useApplyToJobPost";

interface CreateJobApplicationsDrawerProps {
	onSuccess: () => void;
}

const CreateJobApplicationsDrawer = forwardRef<
	CreateJobApplicationsCommand,
	CreateJobApplicationsDrawerProps
>(({ onSuccess }, ref) => {
	const [isOpen, setIsOpen] = useState(false);
	const [jobPostId, setJobPostId] = useState("");
	const [jobPostTitle, setJobPostTitle] = useState("");

	const mutation = useApplyToJobPost({
		onSuccess: () => {
			setJobPostId("");
			setJobPostTitle("");

			setIsOpen(false);
			onSuccess();
		},
	});

	useImperativeHandle(
		ref,
		() => ({
			create: (postId: string, postTitle: string) => {
				setJobPostId(postId);
				setJobPostTitle(postTitle);

				mutation.reset();
				setIsOpen(true);
			},
		}),
		[mutation],
	);

	const handleSave = useCallback(
		(value: Parameters<typeof mutation.mutate>[0]["request"]) => {
			mutation.mutate({
				id: jobPostId,
				request: value,
			});
		},
		[mutation, jobPostId],
	);

	const handleClose = useCallback(() => {
		if (mutation.isPending) {
			return;
		}
		setJobPostId("");
		setJobPostTitle("");

		setIsOpen(false);
	}, [mutation]);

	return (
		<Drawer
			open={isOpen}
			title="Create job applications"
			onClose={handleClose}
			footer={
				<SaveChangesButton
					form="job-applications-form"
					isPending={mutation.isPending}
					wait={mutation.waiting}
				/>
			}
		>
			<DetailOverviewHeader title={jobPostTitle} description="Create applications for job post" />

			<JobApplicationsForm
				initialValue={emptyJobApplications}
				onSubmit={handleSave}
				error={mutation.error}
				isSubmitting={mutation.isPending}
			/>
		</Drawer>
	);
});

CreateJobApplicationsDrawer.displayName = "CreateJobApplicationsDrawer";

export default CreateJobApplicationsDrawer;
