import { Clock } from "lucide-react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import { emptyPosition } from "../schema";

/**
 * The hours and the place. Both go into the contract word for word, and both are what the client
 * asks about on the first morning — which is why they sit on the role rather than being retyped on
 * every person's paperwork.
 */
export const ScheduleStep = withForm({
	defaultValues: emptyPosition,

	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={Clock}
					title="Time and place"
					description="How much work, when it starts, and where. One project often runs on several sites, so a role can carry its own address."
				/>

				<div className="form-wizard__grid">
					<form.AppField name="weeklyHours">
						{(field) => (
							<field.FormInput
								label="Weekly hours"
								placeholder="e.g. 40"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>

					<form.AppField name="workStartsAt">
						{(field) => (
							<field.FormInput
								label="Work starts at"
								type="time"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>
				</div>

				<form.AppField name="workSchedule">
					{(field) => (
						<field.FormTextAreaInput
							label="Working pattern"
							placeholder="e.g. One shift, Monday to Friday, every other Saturday"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
							rows={3}
						/>
					)}
				</form.AppField>

				<div className="form-hint">
					Leave the address empty to use the project's own workplace. Filled in, it is what goes on
					the contract and on the posting notification.
				</div>

				<div className="form-wizard__grid">
					<form.AppField name="street">
						{(field) => (
							<field.FormInput
								label="Street"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>

					<form.AppField name="buildingNumber">
						{(field) => (
							<field.FormInput
								label="Building number"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>

					<form.AppField name="unitNumber">
						{(field) => (
							<field.FormInput
								label="Unit number"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>

					<form.AppField name="postalCode">
						{(field) => (
							<field.FormInput
								label="Postal code"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>

					<form.AppField name="city">
						{(field) => (
							<field.FormInput
								label="City"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>

					<form.AppField name="countryCode">
						{(field) => (
							<field.FormCountrySelect
								label="Country"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>
				</div>
			</FormWizard.Section>
		);
	},
});
