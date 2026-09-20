import { Plus, Trash2 } from "lucide-react";
import type { BankAccountData, BankAccountPurpose, CurrencyCode } from "#/api/models";
import { Button, Input, Select } from "#/components/ui";
import { bankAccountPurposes, currencyCodes } from "../types";

interface BankAccountsFieldProps {
	values: BankAccountData[];
	error?: string | null;
	disabled?: boolean;
	onChange: (values: BankAccountData[]) => void;
}

const emptyAccount: BankAccountData = {
	purpose: "Incoming",
	currency: "PLN",
	iban: "",
	bic: null,
	bankName: null,
};

/**
 * Accounts are identified by what they are for and in which currency, so those two are what the row
 * leads with. A pair may appear only once - the backend refuses a second one, and the duplicate is
 * marked here as soon as it is picked rather than on save.
 */
export function BankAccountsField({ values, error, disabled, onChange }: BankAccountsFieldProps) {
	const accounts = values ?? [];

	const update = (index: number, patch: Partial<BankAccountData>) =>
		onChange(accounts.map((account, i) => (i === index ? { ...account, ...patch } : account)));

	const duplicates = new Set(
		accounts
			.map((account) => `${account.purpose}-${account.currency}`)
			.filter((key, index, all) => all.indexOf(key) !== index),
	);

	return (
		<div className="form-field">
			<span className="form-label">Bank accounts</span>

			<div className="form-hint">
				One account per purpose and currency. The incoming one is what an invoice will quote.
			</div>

			<div className="legal-entity-accounts">
				{accounts.map((account, index) => {
					const isDuplicate = duplicates.has(`${account.purpose}-${account.currency}`);

					return (
						// Rows have no id of their own until they are saved, and two rows may be
						// identical while being typed - the position is the only stable handle.
						<div key={index} className="legal-entity-account-row">
							<Select
								aria-label="Purpose"
								value={account.purpose}
								disabled={disabled}
								onChange={(event) =>
									update(index, { purpose: event.target.value as BankAccountPurpose })
								}
							>
								{Object.entries(bankAccountPurposes).map(([value, label]) => (
									<option key={value} value={value}>
										{label}
									</option>
								))}
							</Select>

							<Select
								aria-label="Currency"
								value={account.currency}
								disabled={disabled}
								onChange={(event) =>
									update(index, { currency: event.target.value as CurrencyCode })
								}
							>
								{Object.entries(currencyCodes).map(([value, label]) => (
									<option key={value} value={value}>
										{label}
									</option>
								))}
							</Select>

							<Input
								aria-label="IBAN"
								placeholder="IBAN"
								value={account.iban}
								disabled={disabled}
								onChange={(event) => update(index, { iban: event.target.value })}
							/>

							<Input
								aria-label="BIC"
								placeholder="BIC (optional)"
								value={account.bic ?? ""}
								disabled={disabled}
								onChange={(event) => update(index, { bic: event.target.value || null })}
							/>

							<Input
								aria-label="Bank"
								placeholder="Bank (optional)"
								value={account.bankName ?? ""}
								disabled={disabled}
								onChange={(event) => update(index, { bankName: event.target.value || null })}
							/>

							<Button
								variant="ghost"
								icon={<Trash2 size={15} />}
								disabled={disabled}
								aria-label="Remove account"
								onClick={() => onChange(accounts.filter((_, i) => i !== index))}
							/>

							{isDuplicate ? (
								<p className="form-error legal-entity-account-error">
									This purpose and currency is already used by another account.
								</p>
							) : null}
						</div>
					);
				})}
			</div>

			<Button
				variant="ghost"
				icon={<Plus size={15} />}
				disabled={disabled}
				onClick={() => onChange([...accounts, { ...emptyAccount }])}
			>
				Add account
			</Button>

			{error ? <p className="form-error">{error}</p> : null}
		</div>
	);
}
