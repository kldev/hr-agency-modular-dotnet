import { Check, X } from "lucide-react";
import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import type { ProjectProjection, ProjectStatus } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useGetCompany } from "#/features/companies/pages/hooks";
import { useAppForm } from "#/forms";
import { useChangeProjectStatus } from "../pages/hooks";
import { allowedProjectStatusTransitions, projectStatuses } from "../types";
import type { ChangeProjectStatusFormCommand } from "./ProjectFormCommand";

interface ChangeProjectStatusDrawerProps {
	onSuccess: () => void;
}

const statusSchema = z.object({
	status: z.string().min(1, "Pick a status"),
	reason: z.string().trim().max(500, "The reason cannot exceed 500 characters."),
});

type GoLiveCheck = {
	label: string;
	met: boolean;
};

/**
 * The same three conditions the handler checks before a project goes live, shown before the save
 * instead of after it. Reported one by one, because "not ready" tells nobody what to go and do.
 */
function GoLiveChecklist({ checks }: { checks: GoLiveCheck[] }) {
	return (
		<div className="project-golive-checklist">
			<p className="form-label">Before going live</p>

			<ul>
				{checks.map((check) => (
					<li key={check.label} className={check.met ? "is-met" : "is-missing"}>
						{check.met ? <Check size={14} /> : <X size={14} />}
						<span>{check.label}</span>
					</li>
				))}
			</ul>
		</div>
	);
}

const FormContent: React.FC<{
	project: ProjectProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ project, onSuccess, handleClose }) => {
	const companyQuery = useGetCompany(project.companyId);

	const { mutation, waiting } = useChangeProjectStatus({
		onSuccess: () => {
			mutation.reset();
			toast.success("Status changed");
			onSuccess();
			handleClose();
		},
	});

	const targets = allowedProjectStatusTransitions[project.status];

	const options = Object.fromEntries(
		targets.map((status) => [status, projectStatuses[status]]),
	) as Record<ProjectStatus, string>;

	const form = useAppForm({
		defaultValues: {
			status: (targets[0] ?? "") as string,
			reason: "",
		},

		validators: {
			onChange: statusSchema,
		},

		onSubmit: async ({ value }) => {
			mutation.mutate({
				projectId: project.id,
				request: {
					status: value.status as ProjectStatus,
					reason: value.reason.trim() || null,
				},
			});
		},
	});

	const checks: GoLiveCheck[] = [
		{ label: "The contract is signed", met: Boolean(project.contract?.isSigned) },
		{
			label: "Somebody is responsible on the client side",
			met: Boolean(project.responsibleContact),
		},
		{
			label: "The client profile is complete",
			met: Boolean(companyQuery.data?.isProfileComplete),
		},
	];

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Change status"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						{targets.length === 0 ? (
							<div className="form-hint">
								This project is {projectStatuses[project.status].toLowerCase()} and does not change
								status any more. A finished project does not restart - the history would stop saying
								that the cooperation ended.
							</div>
						) : (
							<>
								<form.AppField name="status">
									{(field) => (
										<field.FormSelectEnum
											label="New status"
											options={options}
											fieldValue={field.state.value as ProjectStatus}
											errors={field.state.meta.errors}
											fieldName={field.name}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>

								<form.Subscribe selector={(state) => state.values.status}>
									{(status) => (status === "Active" ? <GoLiveChecklist checks={checks} /> : null)}
								</form.Subscribe>

								<form.AppField name="reason">
									{(field) => (
										<field.FormTextAreaInput
											label="Reason"
											rows={3}
											fieldValue={field.state.value}
											errors={field.state.meta.errors}
											fieldName={field.name}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>

								<div className="form-hint">
									The reason is optional, and it is the only thing that will explain this change to
									whoever reads the project a year from now.
								</div>
							</>
						)}

						<ApiError
							error={mutation.error as unknown as Parameters<typeof ApiError>[0]["error"]}
						/>
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					{targets.length > 0 && (
						<form.FormSaveChangesButton wait={waiting} isPending={mutation.isPending} />
					)}
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
};

const ChangeProjectStatusDrawer = forwardRef<
	ChangeProjectStatusFormCommand,
	ChangeProjectStatusDrawerProps
>(({ onSuccess }, ref) => {
	const [project, setProject] = useState<ProjectProjection | null>(null);

	useImperativeHandle(
		ref,
		() => ({
			changeStatus: (target: ProjectProjection) => {
				setProject(target);
			},
		}),
		[],
	);

	if (!project) return null;

	return (
		<FormContent project={project} onSuccess={onSuccess} handleClose={() => setProject(null)} />
	);
});

ChangeProjectStatusDrawer.displayName = "ChangeProjectStatusDrawer";

export default ChangeProjectStatusDrawer;
