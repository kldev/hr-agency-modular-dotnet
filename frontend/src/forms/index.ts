import { createFormHook, createFormHookContexts } from "@tanstack/react-form";

const { fieldContext, formContext, useFormContext, useFieldContext } = createFormHookContexts();

export type FormDateTimeValue = { date: Date | null; time: string };
export { useFieldContext, useFormContext };

import { FormSaveChangesButton } from "#/components/ui";
import {
	FormArrayField,
	FormCheckbox,
	FormChoiceGroup,
	FormCompanyPicker,
	FormCountrySelect,
	FormDatePicker,
	FormDateTime,
	FormInput,
	FormLanguageSelect,
	FormMoneyInput,
	FormMultiChoice,
	FormPasswordInput,
	FormSelectEnum,
	FormTeamPicker,
	FormTextAreaInput,
	FormTimeInput,
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
		FormTeamPicker,
		FormToggle,
		FormDatePicker,
		FormMoneyInput,
		FormPasswordInput,
		FormDateTime,
		FormCountrySelect,
		FormLanguageSelect,
		FormArrayField,
		FormTimeInput,
		FormCheckbox,
		FormMultiChoice,
	},
	formComponents: { FormSaveChangesButton },
	fieldContext,
	formContext,
});
