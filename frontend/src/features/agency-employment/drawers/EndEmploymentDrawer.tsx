import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import type { AgencyEmploymentProjection } from "@/api/models";
import { useEndAgencyEmployment } from "../pages/hooks";
import type { EndEmploymentFormCommand } from "./EmploymentFormCommand";

interface Props {
	onSuccess: () => void;
}

const schema = z.object({
	endsOn: z.string().min(1, "Say when it ends"),
});

const FormContent: React.FC<{
	employment: AgencyEmploymentProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ employment, onSuccess, handleClose }) => {
	const { mutation, waiting } = useEndAgencyEmployment({
		onSuccess: () => {
			mutation.reset();
			toast.success("Engagement ended");
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: { endsOn: "" },

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				userId: employment.userId,
				request: { endsOn: value.endsOn.slice(0, 10) },
			});
		},
	});

	const person = `${employment.user.firstName} ${employment.user.lastName}`;

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`End ${person}'s engagement`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField name="endsOn">
							{(field) => (
								<field.FormDatePicker
									label="Ends on"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						{/*
						 * Ending the engagement is not the same as deleting it: the months already
						 * recorded stay, and somebody who left in March still owes March.
						 */}
						<div className="form-hint">
							Began on {employment.startsOn.slice(0, 10)}. The months already recorded stay where
							they are - ending an engagement does not close anybody's outstanding sheet.
						</div>

						<ApiError
							error={mutation.error as unknown as Parameters<typeof ApiError>[0]["error"]}
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

const EndEmploymentDrawer = forwardRef<EndEmploymentFormCommand, Props>(({ onSuccess }, ref) => {
	const [target, setTarget] = useState<AgencyEmploymentProjection | null>(null);

	useImperativeHandle(ref, () => ({ end: setTarget }), []);

	if (!target) return null;

	return (
		<FormContent employment={target} onSuccess={onSuccess} handleClose={() => setTarget(null)} />
	);
});

EndEmploymentDrawer.displayName = "EndEmploymentDrawer";

export default EndEmploymentDrawer;
