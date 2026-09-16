import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import type { OpportunityProjection, UpdateOpportunityRequest } from "#/api/models";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useGetOpportunity } from "#/features/sales/hooks";
import { useAppForm } from "#/forms";

import type { EditOpportunityRef } from "../SalesCommand";
import {
	EditOpportunityForm,
	type OpportunityEditFormValues,
	opportunityEditSchema,
} from "./EditOpportunityForm";
import { useUpdateOpportuinity } from "./useOpportunityForm";

interface EditOpportunityProps {
	onSuccess: () => void;
}

type RenderFormType = {
	opportunity: OpportunityProjection;
	onSuccess: () => void;
	handleClose: () => void;
};

const FormContent: React.FC<RenderFormType> = ({ opportunity, onSuccess, handleClose }) => {
	const { mutation, waiting } = useUpdateOpportuinity({
		onSuccess: () => {
			onSuccess();
			mutation.reset();
			handleClose();
		},
	});

	const formValues: OpportunityEditFormValues = {
		currency: opportunity.currencyCode,
		description: opportunity.description,
		expectedCloseDate: opportunity.expectedCloseDate,
		expectedValue: opportunity.expectedValue?.toString(),
		isHotLead: opportunity.isHotLead,
		title: opportunity.title,
	};

	const form = useAppForm({
		defaultValues: formValues,

		validators: {
			onChange: opportunityEditSchema,
		},

		onSubmit: async ({ value }) => {
			const request: UpdateOpportunityRequest = {
				...value,
				expectedCloseDate:
					value.expectedCloseDate?.substring(0, value.expectedCloseDate.indexOf("T")) || null,
				expectedValue: Number(value.expectedValue.replace(",", ".")),
			};
			mutation.mutate({ id: opportunity.id, request });
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Edit opportunity"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<EditOpportunityForm
							form={form}
							error={mutation.error}
							isSubmitting={mutation.isPending}
						/>
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					<form.FormSaveChangesButton wait={waiting} isPending={mutation.isPending} />
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
};

const EditOpportunityDrawer = forwardRef<EditOpportunityRef, EditOpportunityProps>(
	({ onSuccess }, ref) => {
		const [opportunityId, setOpportunityId] = useState("");

		const query = useGetOpportunity(opportunityId);

		useImperativeHandle(
			ref,
			() => ({
				edit: (id: string) => {
					setOpportunityId(id);
				},
			}),
			[],
		);

		const opprotunity = query.data;

		if (query.isError) {
			toast.error("Unabled to load opportunity");
		}
		if (!opportunityId || !opprotunity) return null;

		return (
			<FormContent
				opportunity={opprotunity}
				onSuccess={onSuccess}
				handleClose={() => {
					setOpportunityId("");
				}}
			/>
		);
	},
);

EditOpportunityDrawer.displayName = "EditOpportunityDrawer";

export default EditOpportunityDrawer;
