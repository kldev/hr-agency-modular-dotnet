import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { getOrganizationById, updateOrganizationData } from "@/api/endpoints";
import type { OrganizationRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import type { EditOrganizationCommand } from "./OrganizationCommand";
import { OrganizationForm } from "./OrganizationForm";

interface EditOrganizationDrawerProps {
	onSuccess: () => void;
}

const EditOrganizationDrawer = forwardRef<EditOrganizationCommand, EditOrganizationDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [organizationId, setOrganizationId] = useState<string>("");

		const queryClient = useQueryClient();

		const organizationQuery = useQuery({
			queryKey: ["organization", organizationId],
			queryFn: () => getOrganizationById(organizationId),
			enabled: isOpen && organizationId.length > 0,
		});

		const { wait, waiting } = useProjectionWait();

		const updateMutation = useMutation({
			mutationFn: ({ id, request }: { id: string; request: OrganizationRequest }) =>
				updateOrganizationData(id, request),

			onSuccess: async () => {
				await wait();

				queryClient.invalidateQueries({
					queryKey: ["organization", organizationId],
				});

				setIsOpen(false);
				setOrganizationId("");

				onSuccess();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				edit: (id: string) => {
					queryClient.invalidateQueries({
						queryKey: ["organization", id],
					});

					setOrganizationId(id);
					updateMutation.reset();
					setIsOpen(true);
				},
			}),
			[updateMutation],
		);

		const handleSave = useCallback(
			(value: OrganizationRequest) => {
				updateMutation.mutate({ id: organizationId, request: value });
			},
			[updateMutation],
		);

		const handleClose = useCallback(() => {
			if (updateMutation.isPending) {
				return;
			}

			setOrganizationId("");
			updateMutation.reset();
			setIsOpen(false);
		}, [updateMutation]);

		const organizationData = organizationQuery.data;

		return (
			<Drawer
				open={isOpen}
				title="Edit organization"
				onClose={handleClose}
				footer={
					<SaveChangesButton
						form="organization-form"
						isPending={updateMutation.isPending}
						wait={waiting}
					/>
				}
			>
				{organizationQuery.isLoading && <div className="form-loading">Loading organization...</div>}

				{organizationQuery.isError && (
					<div className="form-error" role="alert">
						Unable to load organization.
					</div>
				)}

				{organizationData ? (
					<OrganizationForm
						initialValue={{
							emailDomains: organizationData.emailDomains ?? [],
							name: organizationData.name,
							slug: organizationData.slug,
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

EditOrganizationDrawer.displayName = "EditOrganizationDrawer";

export default EditOrganizationDrawer;
