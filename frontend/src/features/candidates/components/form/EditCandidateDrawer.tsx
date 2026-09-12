import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { getCandidate, updateCandidate } from "@/api/endpoints";
import type { CreateCandidateRequest, UpdateCandidateRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import { CandidateForm } from "./CandidateForm";
import type { EditCandidateFormCommand } from "./CandidateFormCommand";

interface EditCandidateDrawerProps {
	onSuccess: () => void;
}

type EditCandidate = UpdateCandidateRequest & {
	email: string;
	source: CreateCandidateRequest["source"];
};

const EditCandidateDrawer = forwardRef<EditCandidateFormCommand, EditCandidateDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [candidateId, setCandidateId] = useState<string | null>(null);
		const [candidate, setCandidate] = useState<EditCandidate | null>(null);
		const { wait, waiting } = useProjectionWait();

		const updateMutation = useMutation({
			mutationFn: ({ id, request }: { id: string; request: UpdateCandidateRequest }) =>
				updateCandidate(id, request),

			onSuccess: async () => {
				await wait();
				setIsOpen(false);
				setCandidate(null);
				setCandidateId(null);
				onSuccess();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				edit: async (id: string) => {
					updateMutation.reset();

					const data = await getCandidate(id);

					setCandidateId(id);
					setCandidate({
						email: data.email,
						source: data.source,
						firstName: data.firstName,
						lastName: data.lastName,
						phone: data.phoneNumber,
						note: data.note,
					});
					setIsOpen(true);
				},
			}),
			[updateMutation],
		);

		const handleSave = useCallback(
			(data: CreateCandidateRequest | UpdateCandidateRequest) => {
				if (!candidateId) {
					return;
				}

				const value = data as UpdateCandidateRequest;

				updateMutation.mutate({
					id: candidateId,
					request: {
						firstName: value.firstName,
						lastName: value.lastName,
						phone: value.phone,
						note: value.note,
					},
				});
			},
			[candidateId, updateMutation],
		);

		const handleClose = useCallback(() => {
			if (updateMutation.isPending) {
				return;
			}

			updateMutation.reset();
			setIsOpen(false);
			setCandidate(null);
			setCandidateId(null);
		}, [updateMutation]);

		return (
			<Drawer
				open={isOpen}
				title="Edit candidate"
				onClose={handleClose}
				footer={
					<SaveChangesButton
						form="candidate-form"
						isPending={updateMutation.isPending}
						wait={waiting}
					/>
				}
			>
				{candidate && (
					<CandidateForm
						initialValue={candidate}
						mode="edit"
						onSubmit={handleSave}
						error={updateMutation.error}
						isSubmitting={updateMutation.isPending}
					/>
				)}
			</Drawer>
		);
	},
);

EditCandidateDrawer.displayName = "EditCandidateDrawer";

export default EditCandidateDrawer;
