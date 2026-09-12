import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { getCompany, updateCompany as updateCompanyApi } from "@/api/endpoints";
import type { UpdateCompanyRequest } from "@/api/models";
import { Button } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import type { EditCompanyFormCommand } from "../CompanyFormCommand";
import { EditCompanyForm } from "./EditCompanyForm";

type EditCompanyDrawerProps = {
	onSuccess: () => void;
};

const EditCompanyDrawer = forwardRef<EditCompanyFormCommand, EditCompanyDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [companyId, setCompanyId] = useState<string | null>(null);

		const queryClient = useQueryClient();

		const companyQuery = useQuery({
			queryKey: ["company", companyId],
			queryFn: () => getCompany(companyId as string),
			enabled: isOpen && companyId !== null,
		});

		const updateCompany = useMutation({
			mutationFn: ({ companyId, request }: { companyId: string; request: UpdateCompanyRequest }) =>
				updateCompanyApi(companyId, request),

			onSuccess: () => {
				queryClient.invalidateQueries({
					queryKey: ["company", companyId],
				});
				setIsOpen(false);
				setCompanyId(null);

				onSuccess();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				edit: (id: string) => {
					updateCompany.reset();
					setCompanyId(id);
					setIsOpen(true);
				},
			}),
			[updateCompany],
		);

		const handleSave = useCallback(
			(data: UpdateCompanyRequest) => {
				if (!companyId) {
					return;
				}

				updateCompany.mutate({
					companyId,
					request: data,
				});
			},
			[companyId, updateCompany],
		);

		const handleClose = useCallback(() => {
			if (updateCompany.isPending) {
				return;
			}

			updateCompany.reset();
			setIsOpen(false);
			setCompanyId(null);
		}, [updateCompany]);

		const company = companyQuery.data;

		return (
			<Drawer
				open={isOpen}
				title="Edit company"
				onClose={handleClose}
				footer={
					<Button
						variant="primary"
						type="submit"
						form="edit-company-form"
						disabled={companyQuery.isLoading || companyQuery.isError || updateCompany.isPending}
					>
						{updateCompany.isPending ? "Saving..." : "Save changes"}
					</Button>
				}
			>
				{companyQuery.isLoading && <div className="form-loading">Loading company...</div>}

				{companyQuery.isError && (
					<div className="form-error" role="alert">
						Unable to load company.
					</div>
				)}

				{company && (
					<EditCompanyForm
						key={company.id}
						initialValue={{
							name: company.name,
							registrationNumber: company.registrationNumber,
							industry: company.industry,
							webSite: company.website,
							countryCode: company.countryCode,
						}}
						taxId={company.taxId}
						onSubmit={handleSave}
						error={updateCompany.error}
						isSubmitting={updateCompany.isPending}
					/>
				)}
			</Drawer>
		);
	},
);

EditCompanyDrawer.displayName = "EditCompanyDrawer";

export default EditCompanyDrawer;
