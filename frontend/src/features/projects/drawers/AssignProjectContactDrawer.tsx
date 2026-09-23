import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import type { ContactRole, ProjectProjection } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { CompanyContactsPicker } from "#/components/ui/pickers";
import { useAppForm } from "#/forms";
import { useAssignProjectContact } from "../pages/hooks";
import { agencySideContactRoles, contactRoleDescriptions, contactRoles } from "../types";
import type { AssignProjectContactFormCommand } from "./ProjectFormCommand";

interface AssignProjectContactDrawerProps {
	onSuccess: () => void;
}

type AssignTarget = {
	project: ProjectProjection;
	role: ContactRole;
};

const contactSchema = z.object({
	firstName: z.string().trim().min(1, "First name is required"),
	lastName: z.string().trim().min(1, "Last name is required"),
	email: z.string().trim().email("Enter a valid e-mail address"),
	phone: z.string().trim().min(1, "Phone is required"),
	jobTitle: z.string().trim().min(1, "Job title is required"),
	companyContactId: z.string(),
});

const FormContent: React.FC<{
	target: AssignTarget;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ target, onSuccess, handleClose }) => {
	const [pickerQuery, setPickerQuery] = useState("");

	const assigned = target.project.contacts.find((contact) => contact.role === target.role);
	const isAgencySide = agencySideContactRoles.includes(target.role);

	const { mutation, waiting } = useAssignProjectContact({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: {
			firstName: assigned?.person.firstName ?? "",
			lastName: assigned?.person.lastName ?? "",
			email: assigned?.person.email ?? "",
			phone: assigned?.person.phone ?? "",
			jobTitle: assigned?.person.jobTitle ?? "",
			companyContactId: assigned?.companyContactId ?? "",
		},

		validators: {
			onChange: contactSchema,
		},

		onSubmit: async ({ value }) => {
			mutation.mutate({
				projectId: target.project.id,
				role: target.role,
				request: {
					person: {
						firstName: value.firstName.trim(),
						lastName: value.lastName.trim(),
						email: value.email.trim(),
						phone: value.phone.trim(),
						jobTitle: value.jobTitle.trim(),
					},
					companyContactId: value.companyContactId || null,
				},
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`${assigned ? "Change" : "Assign"} ${contactRoles[target.role].toLowerCase()}`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<div className="form-hint">{contactRoleDescriptions[target.role]}</div>

						{/*
						 * The picker only makes sense for the client's own people. The authorised recipient is
						 * ours - looking for them among the client's contacts would be looking in the wrong
						 * address book.
						 */}
						{!isAgencySide ? (
							<div className="form-field">
								<label className="form-label" htmlFor="companyContactId">
									Copy from the client's contacts
								</label>

								<CompanyContactsPicker
									id="companyContactId"
									companyId={target.project.companyId}
									value={form.state.values.companyContactId || null}
									inputValue={pickerQuery}
									onInputChange={setPickerQuery}
									onChange={(value, item) => {
										form.setFieldValue("companyContactId", value ?? "");

										if (!item) return;

										form.setFieldValue("firstName", item.contact.firstName);
										form.setFieldValue("lastName", item.contact.lastName);
										form.setFieldValue("email", item.contact.email);
										form.setFieldValue("phone", item.contact.phone);
										form.setFieldValue("jobTitle", item.contact.jobTitle);
									}}
								/>
							</div>
						) : null}

						<form.AppField name="firstName">
							{(field) => (
								<field.FormInput
									label="First name"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="lastName">
							{(field) => (
								<field.FormInput
									label="Last name"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="jobTitle">
							{(field) => (
								<field.FormInput
									label="Job title"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="email">
							{(field) => (
								<field.FormInput
									label="E-mail"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="phone">
							{(field) => (
								<field.FormInput
									label="Phone"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						{assigned ? (
							<div className="form-hint">
								A role holds one person. Saving replaces{" "}
								{assigned.person.fullname ?? "whoever holds it"}.
							</div>
						) : null}

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

const AssignProjectContactDrawer = forwardRef<
	AssignProjectContactFormCommand,
	AssignProjectContactDrawerProps
>(({ onSuccess }, ref) => {
	const [target, setTarget] = useState<AssignTarget | null>(null);

	useImperativeHandle(
		ref,
		() => ({
			assign: (project: ProjectProjection, role: ContactRole) => {
				setTarget({ project, role });
			},
		}),
		[],
	);

	if (!target) return null;

	return <FormContent target={target} onSuccess={onSuccess} handleClose={() => setTarget(null)} />;
});

AssignProjectContactDrawer.displayName = "AssignProjectContactDrawer";

export default AssignProjectContactDrawer;
