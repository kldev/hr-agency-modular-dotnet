import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { createCompany } from "@/api/endpoints";
import type { CreateCompanyRequest } from "@/api/models";
import { Button } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import type { CreateCompanyFormCommand } from "../CompanyFormCommand";
import { CreateCompanyForm, emptyForm } from "./CreateCompanyForm";

type CraeteCompanyDrawerProps = {
	onSuccess: () => void;
};

const CraeteCompanyDrawer = forwardRef<CreateCompanyFormCommand, CraeteCompanyDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [company, setCompany] = useState<CreateCompanyRequest>(emptyForm);

		const createCompanyMutation = useMutation({
			mutationFn: (request: CreateCompanyRequest) => createCompany(request),
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
					createCompanyMutation.reset();
					setCompany(emptyForm);
					setIsOpen(true);
				},
			}),
			[createCompany],
		);

		const handleSave = useCallback(
			(data: CreateCompanyRequest) => {
				createCompanyMutation.mutate(data);
			},
			[createCompany],
		);

		const handleClose = useCallback(() => {
			if (createCompanyMutation.isPending) {
				return;
			}

			createCompanyMutation.reset();
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
						disabled={createCompanyMutation.isPending}
					>
						{createCompanyMutation.isPending ? "Creating..." : "Create company"}
					</Button>
				}
			>
				<CreateCompanyForm
					initialValue={company}
					onSubmit={handleSave}
					error={createCompanyMutation.error}
					isSubmitting={createCompanyMutation.isPending}
				/>
			</Drawer>
		);
	},
);

CraeteCompanyDrawer.displayName = "CraeteCompanyDrawer";

export default CraeteCompanyDrawer;
