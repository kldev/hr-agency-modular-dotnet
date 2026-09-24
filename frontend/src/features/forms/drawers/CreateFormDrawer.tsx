import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import type { BadRequestDetails, FormDefinitionCreated, FormKind } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useCreateForm } from "../hooks";
import { formKindDescriptions, formKinds } from "../types";

export interface CreateFormCommand {
	create: () => void;
}

/* Mirrors `FormCode` and `FormName`: the backend refuses the same, in the same words. */
const createSchema = z.object({
	name: z
		.string()
		.trim()
		.min(1, "A form needs a name.")
		.max(200, "A form name cannot exceed 200 characters."),
	code: z
		.string()
		.trim()
		.min(1, "A form needs a code.")
		.max(64, "A form code cannot exceed 64 characters.")
		.regex(
			/^[a-z][a-z0-9]*(-[a-z0-9]+)*$/,
			"A form code is lower-case letters, digits and hyphens, starting with a letter, e.g. gdpr-consent.",
		),
	description: z.string().trim().max(2000, "A description cannot exceed 2000 characters."),
	kind: z.string().min(1, "Pick what kind of form it is"),
});

/** `Zgoda RODO` -> `zgoda-rodo`: a code offered from the name for as long as nobody types their own. */
function codeFrom(name: string) {
	return name
		.normalize("NFD")
		.replace(/[̀-ͯ]/g, "")
		.replace(/[łŁ]/g, "l")
		.toLowerCase()
		.replace(/[^a-z0-9]+/g, "-")
		.replace(/^-+|-+$/g, "")
		.replace(/^[0-9-]+/, "")
		.slice(0, 64);
}

/**
 * Naming a form is one short decision, so it is a drawer; the layout is built afterwards on the
 * form's own page. The cardinality follows the kind (a document once, a survey many times) and is
 * not asked: it never changes afterwards, and the default is the right answer nearly always.
 */
function FormContent({
	onCreated,
	handleClose,
}: {
	onCreated: (created: FormDefinitionCreated) => void;
	handleClose: () => void;
}) {
	const [codeTouched, setCodeTouched] = useState(false);

	const { mutation, waiting } = useCreateForm({
		onSuccess: (created) => {
			handleClose();
			onCreated(created as FormDefinitionCreated);
		},
	});

	const form = useAppForm({
		defaultValues: { name: "", code: "", description: "", kind: "Document" },
		validators: { onChange: createSchema },
		onSubmit: ({ value }) =>
			mutation.mutate({
				name: value.name.trim(),
				code: value.code.trim(),
				description: value.description.trim() || null,
				kind: value.kind as FormKind,
				cardinality: null,
				subjectKind: "worker",
			}),
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="New form"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField
							name="name"
							listeners={{
								onChange: ({ value }) => {
									if (!codeTouched) form.setFieldValue("code", codeFrom(value));
								},
							}}
						>
							{(field) => (
								<field.FormInput
									label="Name"
									fieldName={field.name}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									handleChange={field.handleChange}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="code">
							{(field) => (
								<field.FormInput
									label="Code (never changes)"
									fieldName={field.name}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									handleChange={(value) => {
										setCodeTouched(true);
										field.handleChange(value);
									}}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="kind">
							{(field) => (
								<field.FormChoiceGroup
									label="Kind"
									columns={2}
									options={formKinds}
									descriptions={formKindDescriptions}
									fieldName={field.name}
									fieldValue={field.state.value as FormKind}
									errors={field.state.meta.errors}
									handleChange={field.handleChange}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="description">
							{(field) => (
								<field.FormTextAreaInput
									label="Description"
									rows={3}
									fieldName={field.name}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									handleChange={field.handleChange}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<ApiError error={mutation.error as unknown as BadRequestDetails | null} />
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					<form.FormSaveChangesButton wait={waiting} isPending={mutation.isPending} />
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
}

const CreateFormDrawer = forwardRef<
	CreateFormCommand,
	{ onCreated: (created: FormDefinitionCreated) => void }
>(({ onCreated }, ref) => {
	const [open, setOpen] = useState(false);

	useImperativeHandle(ref, () => ({ create: () => setOpen(true) }), []);

	return open ? <FormContent onCreated={onCreated} handleClose={() => setOpen(false)} /> : null;
});

CreateFormDrawer.displayName = "CreateFormDrawer";

export { CreateFormDrawer };
