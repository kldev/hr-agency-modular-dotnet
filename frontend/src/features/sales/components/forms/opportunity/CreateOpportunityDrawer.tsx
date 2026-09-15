import { forwardRef, useCallback, useImperativeHandle, useState } from "react";

import type { CreateOpportunityRequest } from "@/api/models";
import { DetailOverviewHeader, SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import type { CreateOpportunityRef, InitialCompanyOptions } from "../SalesCommand";
import { OpportunityForm } from "./OpportunityForm";
import { useCreateOpportuinity } from "./useOpportunityForm";

interface CreateOpportunityDrawerProps {
	onSuccess: () => void;
}

const CreateOpportunityDrawer = forwardRef<CreateOpportunityRef, CreateOpportunityDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [initial, setInitial] = useState<InitialCompanyOptions | null>(null);

		const { mutation, waiting } = useCreateOpportuinity({
			onSuccess: () => {
				onSuccess();
				setInitial(null);

				mutation.reset();
				setIsOpen(false);
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				create: (initial?: InitialCompanyOptions) => {
					setInitial(initial || null);

					mutation.reset();
					setIsOpen(true);
				},
			}),
			[mutation],
		);

		const handleSave = useCallback(
			(value: CreateOpportunityRequest) => {
				mutation.mutate({ request: value });
			},
			[mutation],
		);

		const handleClose = useCallback(() => {
			if (mutation.isPending) {
				return;
			}

			setInitial(null);

			mutation.reset();
			setIsOpen(false);
		}, [mutation]);

		return (
			<Drawer
				open={isOpen}
				title="Create opportunity"
				onClose={handleClose}
				footer={
					<SaveChangesButton
						form="create-opportunity"
						isPending={mutation.isPending}
						wait={waiting}
					/>
				}
			>
				{initial?.companyName?.length ? (
					<DetailOverviewHeader
						className=" mb-4"
						title={initial?.companyName}
						description=""
					></DetailOverviewHeader>
				) : null}

				<OpportunityForm
					companyId={initial?.companyId ?? undefined}
					formId="create-opportunity"
					mode="create"
					onSubmit={handleSave}
					error={mutation.error}
					isSubmitting={mutation.isPending}
				/>
			</Drawer>
		);
	},
);

CreateOpportunityDrawer.displayName = "CreateOpportunityDrawer";

export default CreateOpportunityDrawer;
