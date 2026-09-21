import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import type { OrgUnitRow } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { Select } from "@/components/ui";
import { useAssignOrgUnitHead } from "../pages/hooks";
import { headMustBeAMemberMessage } from "../types";
import type { AssignOrgUnitHeadFormCommand, OrgUnitPerson } from "./OrgUnitFormCommand";

interface AssignOrgUnitHeadDrawerProps {
	onSuccess: () => void;
}

const schema = z.object({
	headUserId: z.string().min(1, "Pick who heads this unit"),
});

type HeadTarget = {
	unit: OrgUnitRow;
	candidates: OrgUnitPerson[];
};

const FormContent: React.FC<{
	target: HeadTarget;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ target, onSuccess, handleClose }) => {
	const { mutation, waiting } = useAssignOrgUnitHead({
		onSuccess: () => {
			mutation.reset();
			toast.success("Head assigned");
			onSuccess();
			handleClose();
		},
	});

	const isEmpty = target.candidates.length === 0;

	const form = useAppForm({
		defaultValues: { headUserId: target.unit.headUserId ?? "" },

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			if (value.headUserId === target.unit.headUserId) {
				handleClose();
				return;
			}

			mutation.mutate({
				unitId: target.unit.unitId,
				request: { headUserId: value.headUserId },
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`Head of ${target.unit.name}`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField name="headUserId">
							{(field) => (
								<div className="form-field">
									<label className="form-label" htmlFor={field.name}>
										Head
									</label>

									{/*
									 * Only the people already in this unit are offered, which is the rule rather
									 * than a filter over a wider picker: the domain refuses anybody else, and a
									 * search box spanning the whole organization would keep suggesting them.
									 */}
									<Select
										id={field.name}
										name={field.name}
										value={field.state.value}
										disabled={mutation.isPending || isEmpty}
										onChange={(event) => field.handleChange(event.target.value)}
									>
										<option value="">Select a person</option>

										{target.candidates.map((person) => (
											<option key={person.userId} value={person.userId}>
												{person.name}
											</option>
										))}
									</Select>
								</div>
							)}
						</form.AppField>

						<div className="form-hint">
							{isEmpty
								? `${headMustBeAMemberMessage} Nobody is in ${target.unit.name} yet.`
								: headMustBeAMemberMessage}
						</div>

						<ApiError
							error={mutation.error as unknown as Parameters<typeof ApiError>[0]["error"]}
						/>
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					{/* isPending is the only lever on this button, so an impossible save goes through it. */}
					<form.FormSaveChangesButton wait={waiting} isPending={mutation.isPending || isEmpty} />
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
};

const AssignOrgUnitHeadDrawer = forwardRef<
	AssignOrgUnitHeadFormCommand,
	AssignOrgUnitHeadDrawerProps
>(({ onSuccess }, ref) => {
	const [target, setTarget] = useState<HeadTarget | null>(null);

	useImperativeHandle(
		ref,
		() => ({
			assign: (unit: OrgUnitRow, candidates: OrgUnitPerson[]) => {
				setTarget({ unit, candidates });
			},
		}),
		[],
	);

	if (!target) return null;

	return <FormContent target={target} onSuccess={onSuccess} handleClose={() => setTarget(null)} />;
});

AssignOrgUnitHeadDrawer.displayName = "AssignOrgUnitHeadDrawer";

export default AssignOrgUnitHeadDrawer;
