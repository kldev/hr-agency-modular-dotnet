import { createFormHook, createFormHookContexts } from "@tanstack/react-form";

const { fieldContext, formContext } = createFormHookContexts();

import {
	FormCompanyPicker,
	FormDatePicker,
	FormInput,
	FormMoneyInput,
	FormSelectEnum,
	FormTextAreaInput,
	FormToggle,
	FormUserPicker,
} from "./wrapper";

export const { useAppForm } = createFormHook({
	fieldComponents: {
		FormInput,
		FormTextAreaInput,
		FormSelectEnum,
		FormUserPicker,
		FormCompanyPicker,
		FormToggle,
		FormDatePicker,
		FormMoneyInput,
	},
	formComponents: {},
	fieldContext,
	formContext,
});
