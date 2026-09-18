import { forwardRef, useCallback, useImperativeHandle, useState } from "react";

import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useCreateFollowUp, useUpdateFollowUp } from "../opportunity/useOpportunityForm";
import type { FollowUpInfo, FollowUpRef } from "../SalesCommand";
import { emptyFollowUp, FollowUpForm, type FollowUpFormValues, toFormValues } from "./FollowUpForm";

interface FollowUpDrawerProps {
	onSuccess: () => void;
}

type FollowUpTarget =
	| { mode: "add"; opportunityId: string; values: FollowUpFormValues }
	| { mode: "edit"; followUpActionId: string; values: FollowUpFormValues };

const FollowUpDrawer = forwardRef<FollowUpRef, FollowUpDrawerProps>(({ onSuccess }, ref) => {
	const [target, setTarget] = useState<FollowUpTarget | null>(null);

	const createFollowUp = useCreateFollowUp({
		onSuccess: () => {
			setTarget(null);
			createFollowUp.mutation.reset();
			onSuccess();
		},
	});

	const updateFollowUp = useUpdateFollowUp({
		onSuccess: () => {
			setTarget(null);
			updateFollowUp.mutation.reset();
			onSuccess();
		},
	});

	useImperativeHandle(
		ref,
		() => ({
			add: (opportunityId: string) => {
				createFollowUp.mutation.reset();
				setTarget({ mode: "add", opportunityId, values: emptyFollowUp() });
			},

			edit: (info: FollowUpInfo) => {
				updateFollowUp.mutation.reset();
				setTarget({
					mode: "edit",
					followUpActionId: info.followUpActionId,
					values: toFormValues(info.content, info.followDateTime),
				});
			},
		}),
		[createFollowUp.mutation, updateFollowUp.mutation],
	);

	const isPending = createFollowUp.mutation.isPending || updateFollowUp.mutation.isPending;

	const handleSave = useCallback(
		(value: { content: string; followDateTime: string }) => {
			if (!target) {
				return;
			}

			if (target.mode === "add") {
				createFollowUp.mutation.mutate({
					request: { opportunityId: target.opportunityId, ...value },
				});
				return;
			}

			updateFollowUp.mutation.mutate({
				followUpActionId: target.followUpActionId,
				request: value,
			});
		},
		[createFollowUp.mutation, target, updateFollowUp.mutation],
	);

	const handleClose = useCallback(() => {
		if (isPending) {
			return;
		}

		setTarget(null);
		createFollowUp.mutation.reset();
		updateFollowUp.mutation.reset();
	}, [createFollowUp.mutation, isPending, updateFollowUp.mutation]);

	if (!target) return null;

	const isAdd = target.mode === "add";

	return (
		<Drawer
			open={true}
			title={isAdd ? "Schedule follow up" : "Edit follow up"}
			onClose={handleClose}
			footer={
				<SaveChangesButton
					form="follow-up-form"
					isPending={isPending}
					wait={isAdd ? createFollowUp.waiting : updateFollowUp.waiting}
				/>
			}
		>
			<FollowUpForm
				formId="follow-up-form"
				initial={target.values}
				onSubmit={handleSave}
				error={isAdd ? createFollowUp.mutation.error : updateFollowUp.mutation.error}
				isSubmitting={isPending}
			/>
		</Drawer>
	);
});

FollowUpDrawer.displayName = "FollowUpDrawer";

export default FollowUpDrawer;
