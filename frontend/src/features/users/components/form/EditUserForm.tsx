import { ApiError } from "#/components/ui/ApiError";
import { withForm } from "#/forms";
import {
	ContactFields,
	type ContactFieldsValues,
	contactFieldNames,
	contactFieldsSchema,
	emptyContactFields,
} from "./ContactFields";

interface EditUserFormProps {
	error: Error | null;
	isSubmitting: boolean;
}

/**
 * Only what UpdateUserRequest carries. The organization role and the team are changed through their
 * own endpoints, so they get their own actions rather than riding along with a contact edit.
 */
export const editUserSchema = contactFieldsSchema;

export type EditUserFormValues = ContactFieldsValues;

export const EditUserForm = withForm({
	props: {} as EditUserFormProps,
	defaultValues: emptyContactFields,
	render: function Render({ form, isSubmitting, error }) {
		return (
			<>
				<ContactFields form={form} fields={contactFieldNames} isSubmitting={isSubmitting} />

				<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
			</>
		);
	},
});
