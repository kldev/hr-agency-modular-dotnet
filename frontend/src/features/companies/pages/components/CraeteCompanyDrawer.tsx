import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { postApiCompanies } from "@/api/endpoints";
import type { CreateCompanyRequest } from "@/api/models";
import { Button } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import type { CompanyFormCommand } from "./CompanyFormCommand";
import { CreateCompanyForm, emptyForm } from "./CreateCompanyForm";

type CraeteCompanyDrawerProps = {
	onSuccess: () => void;
};

const CraeteCompanyDrawer = forwardRef<CompanyFormCommand, CraeteCompanyDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [company, setCompany] = useState<CreateCompanyRequest>(emptyForm);

		const createCompany = useMutation({
			mutationFn: (request: CreateCompanyRequest) => postApiCompanies(request),
			onSuccess: () => {
				setIsOpen(false);
				setCompany(emptyForm);
				onSuccess();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				create: () => {
					createCompany.reset();
					setCompany(emptyForm);
					setIsOpen(true);
				},
			}),
			[createCompany],
		);

		const handleSave = useCallback(
			(data: CreateCompanyRequest) => {
				createCompany.mutate(data);
			},
			[createCompany],
		);

		const handleClose = useCallback(() => {
			if (createCompany.isPending) {
				return;
			}

			createCompany.reset();
			setIsOpen(false);
		}, [createCompany]);

		return (
			<Drawer
				open={isOpen}
				title="Create company"
				onClose={handleClose}
				footer={
					<Button
						variant="primary"
						type="submit"
						form="company-form"
						disabled={createCompany.isPending}
					>
						{createCompany.isPending ? "Creating..." : "Create company"}
					</Button>
				}
			>
				<CreateCompanyForm
					initialValue={company}
					onSubmit={handleSave}
					error={createCompany.error}
					isSubmitting={createCompany.isPending}
				/>
			</Drawer>
		);
	},
);

CraeteCompanyDrawer.displayName = "CraeteCompanyDrawer";

export default CraeteCompanyDrawer;
