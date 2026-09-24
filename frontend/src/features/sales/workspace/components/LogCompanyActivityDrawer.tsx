import { forwardRef, useImperativeHandle, useState } from "react";
import { SaveChangesButton } from "#/components/ui";
import { Drawer } from "#/components/ui/Drawer";
import { ActivityForm } from "#/features/sales/components/forms/activity/ActivityForm";
import { useLogActivity } from "#/features/sales/components/forms/opportunity/useOpportunityForm";
import { useGetOpportunitesSlice } from "#/features/sales/hooks";

export interface LogCompanyActivityRef {
	open: () => void;
}

interface Props {
	companyId: string;
	onLogged: () => void;
}

const FORM_ID = "log-company-activity";

/**
 * Logging from a screen about a company rather than one deal: the drawer asks which of the
 * company's opportunities it was about, then logs it exactly as the opportunity page does.
 */
export const LogCompanyActivityDrawer = forwardRef<LogCompanyActivityRef, Props>(
	({ companyId, onLogged }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const deals = useGetOpportunitesSlice({ companyId }, { pageSize: 50, enabled: isOpen });

		const { mutation, waiting } = useLogActivity({
			onSuccess: () => {
				mutation.reset();
				setIsOpen(false);
				onLogged();
			},
		});

		useImperativeHandle(ref, () => ({
			open: () => {
				mutation.reset();
				setIsOpen(true);
			},
		}));

		const opportunities = Object.fromEntries(
			(deals.data?.pages.flatMap((page) => page.content) ?? []).map((deal) => [
				deal.id,
				deal.title,
			]),
		);

		const close = () => {
			if (mutation.isPending) return;

			mutation.reset();
			setIsOpen(false);
		};

		return (
			<Drawer
				open={isOpen}
				title="Log activity"
				onClose={close}
				footer={<SaveChangesButton form={FORM_ID} isPending={mutation.isPending} wait={waiting} />}
			>
				{deals.isLoading ? <div className="data-details-loading">Loading ...</div> : null}

				{deals.data && Object.keys(opportunities).length === 0 ? (
					<p className="form-hint">
						This company has no opportunity yet - an activity is always logged against one.
					</p>
				) : null}

				{Object.keys(opportunities).length > 0 ? (
					<ActivityForm
						formId={FORM_ID}
						opportunities={opportunities}
						isSubmitting={mutation.isPending}
						error={mutation.error}
						onSubmit={({ opportunityId, type, note }) =>
							mutation.mutate({ request: { opportunityId: opportunityId as string, type, note } })
						}
					/>
				) : null}
			</Drawer>
		);
	},
);

LogCompanyActivityDrawer.displayName = "LogCompanyActivityDrawer";
