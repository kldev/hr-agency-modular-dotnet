import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import type { ProjectProjection } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useUpdateProject } from "../pages/hooks";
import { toDateOnly } from "../utils";
import type { EditProjectFormCommand } from "./ProjectFormCommand";

interface EditProjectDrawerProps {
	onSuccess: () => void;
}

const editSchema = z
	.object({
		name: z
			.string()
			.trim()
			.min(1, "Name is required")
			.max(200, "Name cannot exceed 200 characters."),
		description: z.string().trim().max(2000, "Description cannot exceed 2000 characters."),
		street: z.string().trim().min(1, "Street is required"),
		buildingNumber: z.string().trim().min(1, "Building number is required"),
		unitNumber: z.string().trim(),
		postalCode: z.string().trim().min(1, "Postal code is required"),
		city: z.string().trim().min(1, "City is required"),
		countryCode: z.string().trim().min(2, "Country is required"),
		startsOn: z.string().min(1, "Start date is required"),
		endsOn: z.string(),
	})
	.superRefine((value, context) => {
		if (value.endsOn && value.endsOn < value.startsOn) {
			context.addIssue({
				code: "custom",
				path: ["endsOn"],
				message: "The end date cannot be earlier than the start date.",
			});
		}
	});

const FormContent: React.FC<{
	project: ProjectProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ project, onSuccess, handleClose }) => {
	const { mutation, waiting } = useUpdateProject({
		onSuccess: () => {
			mutation.reset();
			toast.success("Project updated");
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: {
			name: project.name,
			description: project.description,
			street: project.workplaceAddress.street,
			buildingNumber: project.workplaceAddress.buildingNumber,
			unitNumber: project.workplaceAddress.unitNumber ?? "",
			postalCode: project.workplaceAddress.postalCode,
			city: project.workplaceAddress.city,
			countryCode: project.workplaceAddress.countryCode,
			startsOn: project.startsOn,
			endsOn: project.endsOn ?? "",
		},

		validators: {
			onChange: editSchema,
		},

		onSubmit: async ({ value }) => {
			mutation.mutate({
				projectId: project.id,
				request: {
					name: value.name.trim(),
					description: value.description.trim(),
					street: value.street.trim(),
					buildingNumber: value.buildingNumber.trim(),
					unitNumber: value.unitNumber.trim() || null,
					postalCode: value.postalCode.trim(),
					city: value.city.trim(),
					countryCode: value.countryCode.toUpperCase(),
					startsOn: toDateOnly(value.startsOn),
					endsOn: value.endsOn ? toDateOnly(value.endsOn) : null,
				},
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Edit project"
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
									label="Name"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
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
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
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
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
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
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
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
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
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
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
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
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
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
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<div className="form-hint">
							The country of the place of work decides which compliance requirements the project
							has. Changing it changes that list.
						</div>

						<form.AppField name="startsOn">
							{(field) => (
								<field.FormDatePicker
									label="Starts on"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="endsOn">
							{(field) => (
								<field.FormDatePicker
									label="Ends on"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
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

/**
 * The client and the engagement type are not here on purpose: both are set once, at creation, and
 * `PUT /api/projects/{id}` does not accept them. The team has its own command, so it is not here
 * either.
 */
const EditProjectDrawer = forwardRef<EditProjectFormCommand, EditProjectDrawerProps>(
	({ onSuccess }, ref) => {
		const [project, setProject] = useState<ProjectProjection | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				edit: (target: ProjectProjection) => {
					setProject(target);
				},
			}),
			[],
		);

		if (!project) return null;

		return (
			<FormContent project={project} onSuccess={onSuccess} handleClose={() => setProject(null)} />
		);
	},
);

EditProjectDrawer.displayName = "EditProjectDrawer";

export default EditProjectDrawer;
