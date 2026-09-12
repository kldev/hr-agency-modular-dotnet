import { useMutation, useQuery } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { createCompanyContact, getCompanyContact, updateCompanyContact } from "@/api/endpoints";
import type { CompanyContactRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks/useProjectionWait";
import type { CompanyContactCommand } from "./CompanyContactCommand";
import { CompanyContactForm } from "./CompanyContactForm";

type CompanyContactDrawerProps = {
	onSuccess: () => void;
};

type Mode =
	| {
			type: "create";
			companyId: string;
	  }
	| {
			type: "edit";
			contactId: string;
	  }
	| null;

const emptyForm: CompanyContactRequest = {
	contact: {
		email: "",
		firstName: "",
		lastName: "",
		jobTitle: "",
		phone: "",
	},
	updatePrimary: false,
};

const CompanyContactDrawer = forwardRef<CompanyContactCommand, CompanyContactDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [mode, setMode] = useState<Mode>(null);
		const { wait, waiting } = useProjectionWait();

		const isEdit = mode?.type === "edit";

		const contactQuery = useQuery({
			queryKey: ["company-contact", isEdit ? mode.contactId : null],
			queryFn: () => getCompanyContact(mode?.type === "edit" ? mode.contactId : ""),
			enabled: isOpen && isEdit,
		});

		const createContact = useMutation({
			mutationFn: ({ companyId, request }: { companyId: string; request: CompanyContactRequest }) =>
				createCompanyContact(companyId, request),

			onSuccess: async () => {
				await wait();
				closeDrawer();
				onSuccess();
			},
		});

		const updateContact = useMutation({
			mutationFn: ({ contactId, request }: { contactId: string; request: CompanyContactRequest }) =>
				updateCompanyContact(contactId, request),

			onSuccess: async () => {
				await wait();
				closeDrawer();
				onSuccess();
			},
		});

		const error = createContact.error ?? updateContact.error;

		const closeDrawer = useCallback(() => {
			createContact.reset();
			updateContact.reset();
			contactQuery.refetch;

			setIsOpen(false);
			setMode(null);
		}, [createContact, updateContact]);

		useImperativeHandle(
			ref,
			() => ({
				create: (companyId: string) => {
					createContact.reset();
					updateContact.reset();
					setMode({
						type: "create",
						companyId,
					});
					setIsOpen(true);
				},

				editContact: (contactId: string) => {
					createContact.reset();
					updateContact.reset();
					setMode({
						type: "edit",
						contactId,
					});
					setIsOpen(true);
				},
			}),
			[createContact, updateContact],
		);

		const handleSave = useCallback(
			(request: CompanyContactRequest) => {
				if (!mode) {
					return;
				}

				if (mode.type === "create") {
					createContact.mutate({
						companyId: mode.companyId,
						request,
					});

					return;
				}

				updateContact.mutate({
					contactId: mode.contactId,
					request,
				});
			},
			[mode, createContact, updateContact],
		);

		const initialValue: CompanyContactRequest | null =
			mode?.type === "create"
				? emptyForm
				: contactQuery.data
					? {
							contact: contactQuery.data.contact,
							updatePrimary: false,
						}
					: null;

		const isLoading = isEdit && contactQuery.isLoading;
		const isPending =
			isLoading || createContact.isPending || updateContact.isPaused || !initialValue;

		return (
			<Drawer
				open={isOpen}
				title={isEdit ? "Edit contact" : "Add contact"}
				onClose={closeDrawer}
				footer={
					<SaveChangesButton form="company-contact-form" isPending={isPending} wait={waiting} />
				}
				// footer={
				// 	<Button
				// 		variant="primary"
				// 		type="submit"
				// 		form="company-contact-form"
				// 		disabled={isLoading || createContact.isPending || updateContact.isPaused || !initialValue}
				// 	>
				// 		{createContact.isPending || updateContact.isPaused
				// 			? isEdit
				// 				? "Saving..."
				// 				: "Creating..."
				// 			: isEdit
				// 				? "Save changes"
				// 				: "Add contact"}
				// 	</Button>
				// }
			>
				{isLoading && <div className="form-loading">Loading contact...</div>}

				{contactQuery.isError && (
					<div className="form-error" role="alert">
						Unable to load contact.
					</div>
				)}

				{initialValue && (
					<CompanyContactForm
						key={mode?.type === "edit" ? mode.contactId : mode?.companyId}
						initialValue={initialValue}
						onSubmit={handleSave}
						error={error}
						isSubmitting={createContact.isPending || updateContact.isPaused}
					/>
				)}
			</Drawer>
		);
	},
);

CompanyContactDrawer.displayName = "CompanyContactDrawer";

export default CompanyContactDrawer;
