import { z } from "zod";
import { TeamRole } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { withForm } from "#/forms";
import { emptyMember, TeamMembersField } from "./TeamMembersField";

interface TeamFormProps {
	error: Error | null;
	isSubmitting: boolean;
}

export const teamSchema = z.object({
	name: z.string().trim().min(1, "Name is required").max(100, "Name cannot exceed 100 characters."),

	members: z
		.array(
			z.object({
				userId: z.string().trim().min(1, "Pick a person"),
				role: z.enum(TeamRole),
			}),
		)
		.min(1, "A team needs at least one member.")
		.refine(
			(members) => new Set(members.map((member) => member.userId)).size === members.length,
			"The same person cannot be on the team twice.",
		),
});

export type TeamFormValues = z.infer<typeof teamSchema>;

export const emptyTeam: TeamFormValues = {
	name: "",
	members: [emptyMember],
};

export const TeamForm = withForm({
	props: {} as TeamFormProps,
	defaultValues: emptyTeam,
	render: function Render({ form, isSubmitting, error }) {
		return (
			<>
				<form.AppField name="name">
					{(field) => (
						<field.FormInput
							label="Name"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.Field name="members" mode="array">
					{(field) => (
						<TeamMembersField
							members={field.state.value}
							errors={field.state.meta.errors}
							isSubmitting={isSubmitting}
							onChange={(members) => field.handleChange(members)}
						/>
					)}
				</form.Field>

				<div className="form-hint">
					A person belongs to at most one team, so anyone already on another team has to be moved
					rather than added here.
				</div>

				<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
			</>
		);
	},
});
