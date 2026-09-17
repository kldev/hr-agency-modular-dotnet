import { createFormHook, createFormHookContexts } from "@tanstack/react-form";

const { fieldContext, formContext, useFormContext, useFieldContext } = createFormHookContexts();

export type FormDateTimeValue = { date: Date | null; time: string };
export { useFieldContext, useFormContext };

import { FormSaveChangesButton } from "#/components/ui";
import {
	FormArrayField,
	FormChoiceGroup,
	FormCompanyPicker,
	FormCountrySelect,
	FormDatePicker,
	FormDateTime,
	FormInput,
	FormLanguageSelect,
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
		FormChoiceGroup,
		FormUserPicker,
		FormCompanyPicker,
		FormToggle,
		FormDatePicker,
		FormMoneyInput,
		FormDateTime,
		FormCountrySelect,
		FormLanguageSelect,
		FormArrayField,
	},
	formComponents: { FormSaveChangesButton },
	fieldContext,
	formContext,
});
