import { createFormHook, createFormHookContexts } from "@tanstack/react-form";

const { fieldContext, formContext, useFormContext, useFieldContext } = createFormHookContexts();

export type FormDateTimeValue = { date: Date | null; time: string };
export { useFieldContext, useFormContext };

import { FormSaveChangesButton } from "#/components/ui";
import {
	FormCompanyPicker,
	FormDatePicker,
	FormDateTime,
	FormInput,
	FormMoneyInput,
	FormSelectEnum,
	FormTextAreaInput,
	FormToggle,
	FormUserPicker,
} from "./wrapper";

export const { useAppForm, withForm, withFieldGroup } = createFormHook({
	fieldComponents: {
		FormInput,
		FormTextAreaInput,
		FormSelectEnum,
		FormUserPicker,
		FormCompanyPicker,
		FormToggle,
		FormDatePicker,
		FormMoneyInput,
		FormDateTime,
	},
	formComponents: { FormSaveChangesButton },
	fieldContext,
	formContext,
});
