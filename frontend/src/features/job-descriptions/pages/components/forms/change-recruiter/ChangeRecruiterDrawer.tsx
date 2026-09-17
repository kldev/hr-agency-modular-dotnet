import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { assignJobDescriptionRecruiter } from "@/api/endpoints";
import type { AssignRecruiterRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import type { ChangeRecruiterCommand } from "../JobsDescriptopnCommand";
import { ChangeRecruiterForm, emptyForm } from "./ChangeRecruiterForm";

interface ChangeRecruiterDrawerProps {
	onSuccess: () => void;
}

type Target = {
	jobDescriptionId: string;
	recruiterId: string;
};

const FORM_ID = "change-recruiter-form";

const ChangeRecruiterDrawer = forwardRef<ChangeRecruiterCommand, ChangeRecruiterDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [target, setTarget] = useState<Target | null>(null);

		const { wait, waiting } = useProjectionWait();

		const mutation = useMutation({
			mutationFn: ({ id, request }: { id: string; request: AssignRecruiterRequest }) =>
				assignJobDescriptionRecruiter(id, request),

			onSuccess: async () => {
				await wait();

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
					setTarget({ jobDescriptionId: id, recruiterId });
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
					id: target.jobDescriptionId,
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
