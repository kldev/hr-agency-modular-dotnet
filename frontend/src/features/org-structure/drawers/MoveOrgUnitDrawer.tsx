import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import type { OrgUnitRow } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { Select } from "@/components/ui";
import { useMoveOrgUnit } from "../pages/hooks";
import type { MoveTarget } from "../types";
import type { MoveOrgUnitFormCommand } from "./OrgUnitFormCommand";

interface MoveOrgUnitDrawerProps {
	onSuccess: () => void;
}

const schema = z.object({
	parentId: z.string().min(1, "Pick where the unit should hang"),
});

type MoveTargetState = {
	unit: OrgUnitRow;
	targets: MoveTarget[];
};

const FormContent: React.FC<{
	target: MoveTargetState;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ target, onSuccess, handleClose }) => {
	const { mutation, waiting } = useMoveOrgUnit({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: { parentId: "" },

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				unitId: target.unit.unitId,
				request: { parentId: value.parentId },
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`Move ${target.unit.name}`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField name="parentId">
							{(field) => (
								<div className="form-field">
									<label className="form-label" htmlFor={field.name}>
										New parent
									</label>

									<Select
										id={field.name}
										name={field.name}
										value={field.state.value}
										disabled={mutation.isPending}
										onChange={(event) => field.handleChange(event.target.value)}
									>
										<option value="">Select a unit</option>

										{/*
										 * Everything is listed, and what the domain would refuse comes disabled
										 * with its reason as the hover title. Hiding those rows would leave
										 * somebody hunting for a department that is deliberately not on offer.
										 */}
										{target.targets.map(({ unit, depth, disabledReason }) => (
											<option
												key={unit.unitId}
												value={unit.unitId}
												disabled={disabledReason !== null}
												title={disabledReason ?? undefined}
											>
												{`${"  ".repeat(depth)}${unit.name}`}
												{disabledReason ? " —" : ""}
											</option>
										))}
									</Select>
								</div>
							)}
						</form.AppField>

						<div className="form-hint">
							A unit cannot move under itself or under anything below it - those are greyed out.
							Everybody in the unit moves with it, and so does everybody under them.
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

const MoveOrgUnitDrawer = forwardRef<MoveOrgUnitFormCommand, MoveOrgUnitDrawerProps>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<MoveTargetState | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				move: (unit: OrgUnitRow, targets: MoveTarget[]) => {
					setTarget({ unit, targets });
				},
			}),
			[],
		);

		if (!target) return null;

		return (
			<FormContent target={target} onSuccess={onSuccess} handleClose={() => setTarget(null)} />
		);
	},
);

MoveOrgUnitDrawer.displayName = "MoveOrgUnitDrawer";

export default MoveOrgUnitDrawer;
