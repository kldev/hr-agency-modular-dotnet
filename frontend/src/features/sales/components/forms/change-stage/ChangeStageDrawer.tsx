import { forwardRef, useImperativeHandle, useState } from "react";

import { useAppForm } from "#/forms";
import type { OnSucess } from "#/types";
import { FormDrawer } from "@/components/ui/FormDrawer";
import { useChangeStage } from "../opportunity/useOpportunityForm";
import type { ChangeStageRef, OpportunityInfo } from "../SalesCommand";
import { ChangeStageForm, type ChangeStageFormValues, changeStageSchema } from "./ChangeStageForm";

type RenderFormType = {
	opportunityInfo: OpportunityInfo;
	onSuccess: () => void;
	handleClose: () => void;
};

const ChangeStageContent: React.FC<RenderFormType> = ({
	opportunityInfo,
	onSuccess,
	handleClose,
}) => {
	const { mutation, waiting, changeStage } = useChangeStage({
		onSuccess: () => {
			onSuccess();
			mutation.reset();
		},
	});

	const formValues: ChangeStageFormValues = {
		stage: opportunityInfo.stage,
		lostReason: "",
	};

	const form = useAppForm({
		defaultValues: formValues,

		validators: {
			onChange: changeStageSchema,
		},

		onSubmit: async ({ value }) => {
			changeStage(opportunityInfo.id, value.stage, value.lostReason);
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Change opportunity stage"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<ChangeStageForm
							form={form}
							info={opportunityInfo}
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

const ChangeStageDrawer = forwardRef<ChangeStageRef, OnSucess>(({ onSuccess }, ref) => {
	const [opportunityInfo, setOpportunityInfo] = useState<OpportunityInfo | null>(null);

	useImperativeHandle(
		ref,
		() => ({
			changeStage: (info) => {
				setOpportunityInfo(info);
			},
		}),
		[],
	);

	if (!opportunityInfo) return null;

	return (
		<ChangeStageContent
			onSuccess={() => {
				setOpportunityInfo(null);
				onSuccess();
			}}
			opportunityInfo={opportunityInfo}
			handleClose={() => {
				setOpportunityInfo(null);
			}}
		/>
	);
});

ChangeStageDrawer.displayName = "ChangeStageDrawer";

export default ChangeStageDrawer;
