import { forwardRef, useImperativeHandle, useState } from "react";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import type { CreateOpportunityRequest } from "@/api/models";
import { DetailOverviewHeader } from "@/components/ui";

import type { CreateOpportunityRef, InitialCompanyOptions } from "../SalesCommand";
import { empty, OpportunityForm, opportunitySchema } from "./OpportunityForm";
import { useCreateOpportuinity } from "./useOpportunityForm";

interface CreateOpportunityDrawerProps {
	onSuccess: () => void;
}

type RenderFormType = {
	info?: InitialCompanyOptions;
	onSuccess: () => void;
	handleClose: () => void;
};

const FormContent: React.FC<RenderFormType> = ({ info, onSuccess, handleClose }) => {
	const { mutation, waiting } = useCreateOpportuinity({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: { ...empty, companyId: info?.companyId || "" },

		validators: {
			onChange: opportunitySchema,
		},

		onSubmit: async ({ value }) => {
			const request: CreateOpportunityRequest = {
				...value,
				responsibleId: value.responsibleId || null,
				expectedCloseDate:
					value.expectedCloseDate?.substring(0, value.expectedCloseDate.indexOf("T")) || null,
				expectedValue: Number(value.expectedValue.replace(",", ".")),
			};
			mutation.mutate({ request });
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Create opportunity"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						{info?.companyName?.length ? (
							<DetailOverviewHeader
								className=" mb-4"
								title={info?.companyName}
								description=""
							></DetailOverviewHeader>
						) : null}
						<OpportunityForm
							companyId={info?.companyId}
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

const CreateOpportunityDrawer = forwardRef<CreateOpportunityRef, CreateOpportunityDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [initial, setInitial] = useState<InitialCompanyOptions | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				create: (initial?: InitialCompanyOptions) => {
					setInitial(initial || null);

					setIsOpen(true);
				},
			}),
			[],
		);

		if (!isOpen) return null;

		return (
			<FormContent
				info={initial || undefined}
				onSuccess={onSuccess}
				handleClose={() => {
					setInitial(null);
					setIsOpen(false);
				}}
			/>
		);
	},
);

CreateOpportunityDrawer.displayName = "CreateOpportunityDrawer";

export default CreateOpportunityDrawer;
