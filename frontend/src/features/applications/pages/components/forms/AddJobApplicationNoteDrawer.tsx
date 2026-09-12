import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { addJobApplicationNote } from "@/api/endpoints";
import type { CreateNoteRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";

import { AddJobApplicationNoteForm, emptyCreateNote } from "./AddJobApplicationNoteForm";

export interface AddJobApplicationNoteFormCommand {
	addNote(jobApplicationId: string): void;
}

interface AddJobApplicationNoteDrawerProps {
	onSuccess: () => void;
}

const AddJobApplicationNoteDrawer = forwardRef<
	AddJobApplicationNoteFormCommand,
	AddJobApplicationNoteDrawerProps
>(({ onSuccess }, ref) => {
	const [isOpen, setIsOpen] = useState(false);
	const [jobApplicationId, setJobApplicationId] = useState<string | null>(null);
	const [value, setValue] = useState<CreateNoteRequest>(emptyCreateNote);

	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: ({ id, request }: { id: string; request: CreateNoteRequest }) =>
			addJobApplicationNote(id, request),

		onSuccess: async () => {
			await wait();

			setIsOpen(false);
			setJobApplicationId(null);
			setValue(emptyCreateNote);

			onSuccess();
		},
	});

	useImperativeHandle(
		ref,
		() => ({
			addNote: (id: string) => {
				mutation.reset();
				setJobApplicationId(id);
				setValue(emptyCreateNote);
				setIsOpen(true);
			},
		}),
		[mutation],
	);

	const handleSave = useCallback(
		(data: CreateNoteRequest) => {
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
		setValue(emptyCreateNote);
	}, [mutation]);

	return (
		<Drawer
			open={isOpen}
			title="Add application note"
			onClose={handleClose}
			footer={
				<SaveChangesButton
					form="add-job-application-note-form"
					isPending={mutation.isPending}
					wait={waiting}
				/>
			}
		>
			<AddJobApplicationNoteForm
				initialValue={value}
				onSubmit={handleSave}
				error={mutation.error}
				isSubmitting={mutation.isPending}
			/>
		</Drawer>
	);
});

AddJobApplicationNoteDrawer.displayName = "AddJobApplicationNoteDrawer";

export default AddJobApplicationNoteDrawer;
