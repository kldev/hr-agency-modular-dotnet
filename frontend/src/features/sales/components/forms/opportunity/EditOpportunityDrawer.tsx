import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import type { UpdateOpportunityRequest } from "#/api/models";
import { useGetOpportunity } from "#/features/sales/hooks";
import { DetailOverviewHeader, SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import type { EditOpportunityRef } from "../SalesCommand";
import { EditOpportunityForm } from "./EditOpportunityForm";

import { useUpdateOpportuinity } from "./useOpportunityForm";

interface EditOpportunityProps {
	onSuccess: () => void;
}

const EditOpportunityDrawer = forwardRef<EditOpportunityRef, EditOpportunityProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [opportunityId, setOpportunityId] = useState("");

		const query = useGetOpportunity(opportunityId);

		const { mutation, waiting } = useUpdateOpportuinity({
			onSuccess: () => {
				onSuccess();
				setOpportunityId("");

				mutation.reset();
				setIsOpen(false);
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				edit: (id: string) => {
					setOpportunityId(id);

					mutation.reset();
					setIsOpen(true);
				},
			}),
			[mutation],
		);

		const handleSave = useCallback(
			(value: UpdateOpportunityRequest) => {
				mutation.mutate({ id: opportunityId, request: { ...value } });
			},
			[mutation, opportunityId],
		);

		const handleClose = useCallback(() => {
			if (mutation.isPending) {
				return;
			}

			setOpportunityId("");

			mutation.reset();
			setIsOpen(false);
		}, [mutation]);

		const opprotunity = query.data;

		if (query.isError) {
			toast.error("Unabled to load opportunity");
		}

		return (
			<Drawer
				open={isOpen}
				title="Edit opportunity"
				onClose={handleClose}
				footer={
					<SaveChangesButton
						form="edit-opportunity"
						isPending={mutation.isPending}
						wait={waiting}
					/>
				}
			>
				{opprotunity ? (
					<>
						<DetailOverviewHeader
							className=" mb-4"
							title={opprotunity?.company.name}
							description=""
						></DetailOverviewHeader>

						<EditOpportunityForm
							formId="edit-opportunity"
							initial={{
								currency: opprotunity.currencyCode,
								title: opprotunity.title,
								description: opprotunity.description,
								expectedCloseDate: opprotunity.expectedCloseDate,
								expectedValue: opprotunity.expectedValue as string,
								isHotLead: opprotunity.isHotLead,
							}}
							onSubmit={handleSave}
							error={mutation.error}
							isSubmitting={mutation.isPending}
						/>
					</>
				) : null}
			</Drawer>
		);
	},
);

EditOpportunityDrawer.displayName = "EditOpportunityDrawer";

export default EditOpportunityDrawer;
