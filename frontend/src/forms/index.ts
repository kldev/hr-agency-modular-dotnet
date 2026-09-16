import { createFormHook, createFormHookContexts } from "@tanstack/react-form";

const { fieldContext, formContext } = createFormHookContexts();

export type FormDateTimeValue = { date: Date | null; time: string };

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
		FormDateTime,
	},
	formComponents: {},
	fieldContext,
	formContext,
});
