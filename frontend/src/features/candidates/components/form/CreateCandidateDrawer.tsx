import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { createCandidate } from "@/api/endpoints";
import type { CreateCandidateRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";

import { CandidateForm, emptyCreateCandidate } from "./CandidateForm";
import type { CreateCandidateFormCommand } from "./CandidateFormCommand";

interface CreateCandidateDrawerProps {
	onSuccess: () => void;
}

const CreateCandidateDrawer = forwardRef<CreateCandidateFormCommand, CreateCandidateDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [candidate, setCandidate] = useState<CreateCandidateRequest>(emptyCreateCandidate);
		const { wait, waiting } = useProjectionWait();

		const createMutation = useMutation({
			mutationFn: (request: CreateCandidateRequest) => createCandidate(request),

			onSuccess: async () => {
				await wait();
				setIsOpen(false);
				setCandidate(emptyCreateCandidate);
				onSuccess();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				create: () => {
					createMutation.reset();
					setCandidate(emptyCreateCandidate);
					setIsOpen(true);
				},
			}),
			[createMutation],
		);

		const handleSave = useCallback(
			(data: CreateCandidateRequest) => {
				createMutation.mutate(data);
			},
			[createMutation],
		);

		const handleClose = useCallback(() => {
			if (createMutation.isPending) {
				return;
			}

			createMutation.reset();
			setIsOpen(false);
		}, [createMutation]);

		return (
			<Drawer
				open={isOpen}
				title="Create candidate"
				onClose={handleClose}
				footer={
					<SaveChangesButton
						wait={waiting}
						form="candidate-form"
						isPending={createMutation.isPending}
					/>
				}
			>
				<CandidateForm
					initialValue={candidate}
					mode="create"
					onSubmit={(value) => handleSave(value as CreateCandidateRequest)}
					error={createMutation.error}
					isSubmitting={createMutation.isPending}
				/>
			</Drawer>
		);
	},
);

CreateCandidateDrawer.displayName = "CreateCandidateDrawer";

export default CreateCandidateDrawer;
