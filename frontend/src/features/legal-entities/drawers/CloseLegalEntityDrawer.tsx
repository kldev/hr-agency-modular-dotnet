import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import type { LegalEntityProjection } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useCloseLegalEntity } from "../pages/hooks";

export interface CloseLegalEntityCommand {
	close: (entity: LegalEntityProjection) => void;
}

const FormContent: React.FC<{
	entity: LegalEntityProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ entity, onSuccess, handleClose }) => {
	const { mutation, waiting } = useCloseLegalEntity({
		onSuccess: () => {
			mutation.reset();
			toast.success("Legal entity closed");
			onSuccess();
			handleClose();
		},
	});

	const schema = z
		.object({ activeTo: z.string().min(1, "The last day of trading is required") })
		.refine((value) => value.activeTo >= entity.activeFrom, {
			message: "The last day cannot be earlier than the first one",
			path: ["activeTo"],
		});

	const form = useAppForm({
		defaultValues: { activeTo: "" },

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			mutation.mutate({ id: entity.id, activeTo: value.activeTo });
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`Close ${entity.name}`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<p className="form-hint">
							The entity stops appearing as a choice for new projects from the day after. The
							projects it already runs keep it.
						</p>

						<form.AppField name="activeTo">
							{(field) => (
								<field.FormDatePicker
									label="Last day of trading"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

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

export const CloseLegalEntityDrawer = forwardRef<
	CloseLegalEntityCommand,
	{ onSuccess: () => void }
>(({ onSuccess }, ref) => {
	const [entity, setEntity] = useState<LegalEntityProjection | null>(null);

	useImperativeHandle(ref, () => ({ close: setEntity }), []);

	if (!entity) return null;

	return <FormContent entity={entity} onSuccess={onSuccess} handleClose={() => setEntity(null)} />;
});

CloseLegalEntityDrawer.displayName = "CloseLegalEntityDrawer";
