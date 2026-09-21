import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import { OrgUnitKind, type OrgUnitRow } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useCreateOrgUnit } from "../pages/hooks";
import {
	orgUnitKinds,
	unitNameMaxLength,
	unitNameRequiredMessage,
	unitNameTooLongMessage,
} from "../types";
import type { CreateOrgUnitFormCommand } from "./OrgUnitFormCommand";

interface CreateOrgUnitDrawerProps {
	onSuccess: (unitId: string) => void;
}

const schema = z.object({
	name: z
		.string()
		.trim()
		.min(1, unitNameRequiredMessage)
		.max(unitNameMaxLength, unitNameTooLongMessage),
	kind: z.enum(OrgUnitKind),
});

/** A section under a department, a department under the board - the usual answer, still editable. */
function kindUnder(parent: OrgUnitRow | null): OrgUnitKind {
	if (parent === null) {
		return OrgUnitKind.Board;
	}

	return parent.kind === OrgUnitKind.Board ? OrgUnitKind.Department : OrgUnitKind.Section;
}

const FormContent: React.FC<{
	parent: OrgUnitRow | null;
	onSuccess: (unitId: string) => void;
	handleClose: () => void;
}> = ({ parent, onSuccess, handleClose }) => {
	const { mutation, waiting } = useCreateOrgUnit({
		onSuccess: (created) => {
			mutation.reset();
			toast.success("Unit created");
			onSuccess(created.unitId);
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: { name: "", kind: kindUnder(parent) },

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				request: {
					parentId: parent?.unitId ?? null,
					name: value.name.trim(),
					kind: value.kind,
				},
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={parent ? `New unit under ${parent.name}` : "Create the top unit"}
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

						<form.AppField name="kind">
							{(field) => (
								<field.FormSelectEnum
									label="Kind"
									options={orgUnitKinds}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						{/*
						 * Where the unit hangs is chosen in the tree, not here, so the parent is shown as
						 * settled fact rather than as a second control answering the same question.
						 */}
						<div className="form-hint">
							{parent
								? `It goes under ${parent.name}. Move it later if the company changes shape.`
								: "The top unit is the only one without a parent, and there is exactly one of it. Everything else hangs underneath."}
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

const CreateOrgUnitDrawer = forwardRef<CreateOrgUnitFormCommand, CreateOrgUnitDrawerProps>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<{ parent: OrgUnitRow | null } | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				create: (parent: OrgUnitRow | null) => {
					setTarget({ parent });
				},
			}),
			[],
		);

		if (!target) return null;

		return (
			<FormContent
				parent={target.parent}
				onSuccess={onSuccess}
				handleClose={() => setTarget(null)}
			/>
		);
	},
);

CreateOrgUnitDrawer.displayName = "CreateOrgUnitDrawer";

export default CreateOrgUnitDrawer;
