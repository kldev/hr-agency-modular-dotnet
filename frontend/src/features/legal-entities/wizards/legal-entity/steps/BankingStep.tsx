import { Wallet } from "lucide-react";
import type { BankAccountData } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { BankAccountsField } from "../../../drawers/BankAccountsField";

interface BankingStepProps {
	accounts: BankAccountData[];
	onChange: (accounts: BankAccountData[]) => void;
	isSubmitting: boolean;
}

/*
 * The one step outside the form: bank accounts are a repeatable list held next to it, so this is a
 * plain component rather than a `withForm` step.
 */
export function BankingStep({ accounts, onChange, isSubmitting }: BankingStepProps) {
	return (
		<FormWizard.Section>
			<FormWizard.SectionHeader
				icon={Wallet}
				title="Banking"
				description="Accounts this company invoices from and pays out of. Optional, and easy to add later."
			/>

			<BankAccountsField values={accounts} disabled={isSubmitting} onChange={onChange} />
		</FormWizard.Section>
	);
}
