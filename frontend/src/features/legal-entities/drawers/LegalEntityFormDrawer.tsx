import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import type { BankAccountData, LegalEntityProjection, LegalEntityRequest } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useCreateLegalEntity, useUpdateLegalEntity } from "../pages/hooks";
import { BankAccountsField } from "./BankAccountsField";

export interface LegalEntityFormCommand {
	create: () => void;
	edit: (entity: LegalEntityProjection) => void;
}

const legalEntitySchema = z
	.object({
		name: z.string().trim().min(1, "Name is required"),
		legalName: z.string().trim().min(1, "Registered name is required"),
		taxId: z.string().trim().min(1, "Tax ID is required"),
		vatNumber: z.string(),

		street: z.string().trim().min(1, "Street is required"),
		buildingNumber: z.string().trim().min(1, "Building number is required"),
		unitNumber: z.string(),
		postalCode: z.string().trim().min(1, "Postal code is required"),
		city: z.string().trim().min(1, "City is required"),
		countryCode: z.string().trim().min(1, "Country is required"),

		description: z.string(),

		presidentFirstName: z.string().trim().min(1, "First name is required"),
		presidentLastName: z.string().trim().min(1, "Last name is required"),
		presidentEmail: z.string(),

		activeFrom: z.string().min(1, "The first day of trading is required"),
		activeTo: z.string(),
	})
	.refine((value) => !value.activeTo || value.activeTo >= value.activeFrom, {
		message: "The last day of trading cannot be earlier than the first one",
		path: ["activeTo"],
	});

type LegalEntityFormValues = z.infer<typeof legalEntitySchema>;

const emptyValues: LegalEntityFormValues = {
	name: "",
	legalName: "",
	taxId: "",
	vatNumber: "",
	street: "",
	buildingNumber: "",
	unitNumber: "",
	postalCode: "",
	city: "",
	countryCode: "",
	description: "",
	presidentFirstName: "",
	presidentLastName: "",
	presidentEmail: "",
	activeFrom: "",
	activeTo: "",
};

function toValues(entity: LegalEntityProjection): LegalEntityFormValues {
	return {
		name: entity.name,
		legalName: entity.legalName,
		taxId: entity.taxId,
		vatNumber: entity.vatNumber ?? "",
		street: entity.registeredAddress.street,
		buildingNumber: entity.registeredAddress.buildingNumber,
		unitNumber: entity.registeredAddress.unitNumber ?? "",
		postalCode: entity.registeredAddress.postalCode,
		city: entity.registeredAddress.city,
		countryCode: entity.registeredAddress.countryCode,
		description: entity.description ?? "",
		presidentFirstName: entity.president.firstName,
		presidentLastName: entity.president.lastName,
		presidentEmail: entity.president.email ?? "",
		activeFrom: entity.activeFrom,
		activeTo: entity.activeTo ?? "",
	};
}

const FormContent: React.FC<{
	entity: LegalEntityProjection | null;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ entity, onSuccess, handleClose }) => {
	const [accounts, setAccounts] = useState<BankAccountData[]>(entity?.bankAccounts ?? []);

	const done = () => {
		toast.success(entity ? "Legal entity updated" : "Legal entity created");
		onSuccess();
		handleClose();
	};

	const create = useCreateLegalEntity({
		onSuccess: () => {
			create.mutation.reset();
			done();
		},
	});

	const update = useUpdateLegalEntity({
		onSuccess: () => {
			update.mutation.reset();
			done();
		},
	});

	const active = entity ? update : create;
	const isSubmitting = active.mutation.isPending;

	const form = useAppForm({
		defaultValues: entity ? toValues(entity) : emptyValues,

		validators: { onChange: legalEntitySchema },

		onSubmit: async ({ value }) => {
			const request: LegalEntityRequest = {
				name: value.name.trim(),
				legalName: value.legalName.trim(),
				taxId: value.taxId.trim(),
				vatNumber: value.vatNumber.trim() || null,
				street: value.street.trim(),
				buildingNumber: value.buildingNumber.trim(),
				unitNumber: value.unitNumber.trim() || null,
				postalCode: value.postalCode.trim(),
				city: value.city.trim(),
				countryCode: value.countryCode,
				description: value.description.trim() || null,
				presidentFirstName: value.presidentFirstName.trim(),
				presidentLastName: value.presidentLastName.trim(),
				presidentEmail: value.presidentEmail.trim() || null,
				activeFrom: value.activeFrom,
				activeTo: value.activeTo || null,
				bankAccounts: accounts,
			};

			if (entity) {
				update.mutation.mutate({ id: entity.id, request });
				return;
			}

			create.mutation.mutate(request);
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={entity ? "Edit legal entity" : "New legal entity"}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField name="name">
							{(field) => (
								<field.FormInput
									label="Trading name"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="legalName">
							{(field) => (
								<field.FormInput
									label="Registered name"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="taxId">
							{(field) => (
								<field.FormInput
									label="Tax ID"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="vatNumber">
							{(field) => (
								<field.FormInput
									label="VAT number"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="street">
							{(field) => (
								<field.FormInput
									label="Street"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="buildingNumber">
							{(field) => (
								<field.FormInput
									label="Building number"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="unitNumber">
							{(field) => (
								<field.FormInput
									label="Unit number"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="postalCode">
							{(field) => (
								<field.FormInput
									label="Postal code"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="city">
							{(field) => (
								<field.FormInput
									label="City"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="countryCode">
							{(field) => (
								<field.FormCountrySelect
									label="Country"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="presidentFirstName">
							{(field) => (
								<field.FormInput
									label="President - first name"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="presidentLastName">
							{(field) => (
								<field.FormInput
									label="President - last name"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="presidentEmail">
							{(field) => (
								<field.FormInput
									label="President - email"
									type="email"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="activeFrom">
							{(field) => (
								<field.FormDatePicker
									label="Trading since"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="activeTo">
							{(field) => (
								<field.FormDatePicker
									label="Trading until"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<form.AppField name="description">
							{(field) => (
								<field.FormTextAreaInput
									label="Description"
									rows={3}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<BankAccountsField values={accounts} disabled={isSubmitting} onChange={setAccounts} />

						<ApiError
							error={active.mutation.error as unknown as Parameters<typeof ApiError>[0]["error"]}
						/>
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					<form.FormSaveChangesButton wait={active.waiting} isPending={isSubmitting} />
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
};

export const LegalEntityFormDrawer = forwardRef<LegalEntityFormCommand, { onSuccess: () => void }>(
	({ onSuccess }, ref) => {
		const [state, setState] = useState<{ open: boolean; entity: LegalEntityProjection | null }>({
			open: false,
			entity: null,
		});

		useImperativeHandle(
			ref,
			() => ({
				create: () => setState({ open: true, entity: null }),
				edit: (entity: LegalEntityProjection) => setState({ open: true, entity }),
			}),
			[],
		);

		if (!state.open) return null;

		return (
			<FormContent
				entity={state.entity}
				onSuccess={onSuccess}
				handleClose={() => setState({ open: false, entity: null })}
			/>
		);
	},
);

LegalEntityFormDrawer.displayName = "LegalEntityFormDrawer";
