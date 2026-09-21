import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import type { OrgUnitRow } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useRenameOrgUnit } from "../pages/hooks";
import { unitNameMaxLength, unitNameRequiredMessage, unitNameTooLongMessage } from "../types";
import type { RenameOrgUnitFormCommand } from "./OrgUnitFormCommand";

interface RenameOrgUnitDrawerProps {
	onSuccess: () => void;
}

const schema = z.object({
	name: z
		.string()
		.trim()
		.min(1, unitNameRequiredMessage)
		.max(unitNameMaxLength, unitNameTooLongMessage),
});

const FormContent: React.FC<{
	unit: OrgUnitRow;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ unit, onSuccess, handleClose }) => {
	const { mutation, waiting } = useRenameOrgUnit({
		onSuccess: () => {
			mutation.reset();
			toast.success("Unit renamed");
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: { name: unit.name },

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			mutation.mutate({ unitId: unit.unitId, request: { name: value.name.trim() } });
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Rename unit"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField name="name">
							{(field) => (
								<field.FormInput
									label="Name"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<div className="form-hint">
							Two units under the same parent cannot share a name. Elsewhere in the chart the same
							name is fine - two companies can both have a payroll desk.
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

const RenameOrgUnitDrawer = forwardRef<RenameOrgUnitFormCommand, RenameOrgUnitDrawerProps>(
	({ onSuccess }, ref) => {
		const [unit, setUnit] = useState<OrgUnitRow | null>(null);

		useImperativeHandle(ref, () => ({ rename: setUnit }), []);

		if (!unit) return null;

		return <FormContent unit={unit} onSuccess={onSuccess} handleClose={() => setUnit(null)} />;
	},
);

RenameOrgUnitDrawer.displayName = "RenameOrgUnitDrawer";

export default RenameOrgUnitDrawer;
