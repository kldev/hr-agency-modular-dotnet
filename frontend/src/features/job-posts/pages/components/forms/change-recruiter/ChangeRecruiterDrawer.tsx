import { useMutation, useQueryClient } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { changeJobPostResponsibleRecruiter } from "@/api/endpoints";
import type { AssignRecruiterRequest } from "@/api/models";
import { jobPostsKeys } from "@/api/query-keys";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import type { ChangeJobPostRecruiterCommand } from "../JobPostsCommand";
import { ChangeRecruiterForm, emptyForm } from "./ChangeRecruiterForm";

interface ChangeRecruiterDrawerProps {
	onSuccess: () => void;
}

type Target = {
	jobPostId: string;
	recruiterId: string;
};

const FORM_ID = "change-job-post-recruiter-form";

const ChangeRecruiterDrawer = forwardRef<ChangeJobPostRecruiterCommand, ChangeRecruiterDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [target, setTarget] = useState<Target | null>(null);

		const queryClient = useQueryClient();

		const { wait, waiting } = useProjectionWait();

		const mutation = useMutation({
			mutationFn: ({ id, request }: { id: string; request: AssignRecruiterRequest }) =>
				changeJobPostResponsibleRecruiter(id, request),

			onSuccess: async () => {
				await wait();

				await queryClient.invalidateQueries({ queryKey: jobPostsKeys.all });

				setIsOpen(false);
				setTarget(null);

				onSuccess();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				changeRecruiter: (id: string, recruiterId: string) => {
					mutation.reset();
					setTarget({ jobPostId: id, recruiterId });
					setIsOpen(true);
				},
			}),
			[mutation],
		);

		const handleSave = useCallback(
			(data: AssignRecruiterRequest) => {
				if (!target) {
					return;
				}

				mutation.mutate({
					id: target.jobPostId,
					request: data,
				});
			},
			[target, mutation],
		);

		const handleClose = useCallback(() => {
			if (mutation.isPending) {
				return;
			}

			mutation.reset();
			setIsOpen(false);
			setTarget(null);
		}, [mutation]);

		return (
			<Drawer
				open={isOpen}
				title="Change recruiter"
				onClose={handleClose}
				footer={<SaveChangesButton form={FORM_ID} isPending={mutation.isPending} wait={waiting} />}
			>
				<ChangeRecruiterForm
					initialValue={target ? { recruiterId: target.recruiterId } : emptyForm}
					formId={FORM_ID}
					onSubmit={handleSave}
					error={mutation.error}
					isSubmitting={mutation.isPending}
				/>
			</Drawer>
		);
	},
);

ChangeRecruiterDrawer.displayName = "ChangeRecruiterDrawer";

export default ChangeRecruiterDrawer;
