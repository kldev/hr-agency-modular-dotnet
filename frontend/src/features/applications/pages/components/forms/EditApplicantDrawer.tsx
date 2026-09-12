import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { getJobApplication, updateJobApplicant } from "@/api/endpoints";
import type { UpdateApplicantRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import { ApplicantForm } from "./ApplicantForm";

interface EditApplicantDrawerProps {
	onSuccess: () => void;
}

export interface EditApplicantCommand {
	edit(jobApplicationId: string): void;
}

const EditApplicantDrawer = forwardRef<EditApplicantCommand, EditApplicantDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [applicationId, setApplicationId] = useState<string>("");

		const queryClient = useQueryClient();

		const applicationQuery = useQuery({
			queryKey: ["job-application", applicationId],
			queryFn: () => getJobApplication(applicationId),
			enabled: isOpen && applicationId.length > 0,
		});

		const { wait, waiting } = useProjectionWait();

		const updateMutation = useMutation({
			mutationFn: ({ id, request }: { id: string; request: UpdateApplicantRequest }) =>
				updateJobApplicant(id, request),

			onSuccess: async () => {
				await wait();

				queryClient.invalidateQueries({
					queryKey: ["job-application", applicationId],
				});

				setIsOpen(false);
				setApplicationId("");
				onSuccess();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				edit: (id: string) => {
					queryClient.invalidateQueries({
						queryKey: ["job-application", id],
					});

					setApplicationId(id);
					updateMutation.reset();
					setIsOpen(true);
				},
			}),
			[updateMutation],
		);

		const handleSave = useCallback(
			(value: UpdateApplicantRequest) => {
				updateMutation.mutate({ id: applicationId, request: value });
			},
			[updateMutation],
		);

		const handleClose = useCallback(() => {
			if (updateMutation.isPending) {
				return;
			}

			setApplicationId("");
			updateMutation.reset();
			setIsOpen(false);
		}, [updateMutation]);

		const application = applicationQuery.data;

		return (
			<Drawer
				open={isOpen}
				title="Edit applicant"
				onClose={handleClose}
				footer={
					<SaveChangesButton
						form="applicant-form"
						isPending={updateMutation.isPending}
						wait={waiting}
					/>
				}
			>
				{applicationQuery.isLoading && <div className="form-loading">Loading application...</div>}

				{applicationQuery.isError && (
					<div className="form-error" role="alert">
						Unable to load application.
					</div>
				)}

				{application ? (
					<ApplicantForm
						initialValue={{
							phone: application.applicantPhone ?? "",
							firstName: application.applicantFirstName ?? "",
							lastName: application.applicantLastName ?? "",
						}}
						onSubmit={handleSave}
						error={updateMutation.error}
						isSubmitting={updateMutation.isPending}
					/>
				) : null}
			</Drawer>
		);
	},
);

EditApplicantDrawer.displayName = "EditApplicantDrawer";

export default EditApplicantDrawer;
