import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import type { OrgUnitRow } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useAddOrgUnitMember } from "../pages/hooks";
import type { AddOrgUnitMemberFormCommand } from "./OrgUnitFormCommand";

interface AddOrgUnitMemberDrawerProps {
	onSuccess: () => void;
}

const schema = z.object({
	userId: z.string().trim().min(1, "Pick a person"),
	title: z.string().trim().max(500, "Note cannot exceed 500 characters."),
});

const FormContent: React.FC<{
	unit: OrgUnitRow;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ unit, onSuccess, handleClose }) => {
	const { mutation, waiting } = useAddOrgUnitMember({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: { userId: "", title: "" },

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				unitId: unit.unitId,
				request: { userId: value.userId, title: value.title.trim() || null },
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`Add to ${unit.name}`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField name="userId">
							{(field) => (
								<field.FormUserPicker
									label="Person"
									placeholder="Search people ..."
									fieldValue={{ id: field.state.value }}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val.id ?? "")}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						{/*
						 * Whether somebody already sits elsewhere is left to the backend rather than checked
						 * here: it is a fact about the whole chart, and a refusal that names the other unit is
						 * more use than a picker that quietly drops half the organization.
						 */}
						<div className="form-hint">
							A person belongs to one unit. Somebody already in another one has to be taken out of
							it first - a second entry would split their history in two.
						</div>

						<form.AppField name="title">
							{(field) => (
								<field.FormInput
									label="Title (optional)"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<div className="form-hint">
							For the cases the chart cannot express on its own - a second owner sitting on the
							board with a title of their own, rather than heading it.
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

const AddOrgUnitMemberDrawer = forwardRef<AddOrgUnitMemberFormCommand, AddOrgUnitMemberDrawerProps>(
	({ onSuccess }, ref) => {
		const [unit, setUnit] = useState<OrgUnitRow | null>(null);

		useImperativeHandle(ref, () => ({ add: setUnit }), []);

		if (!unit) return null;

		return <FormContent unit={unit} onSuccess={onSuccess} handleClose={() => setUnit(null)} />;
	},
);

AddOrgUnitMemberDrawer.displayName = "AddOrgUnitMemberDrawer";

export default AddOrgUnitMemberDrawer;
