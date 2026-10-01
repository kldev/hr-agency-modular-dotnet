import { ListBox } from "@heroui/react";
import { FieldError } from "#/components/ui";
import { SelectField } from "#/components/ui/Select";
import { useGetOpportunitesSlice } from "../hooks";

interface OpportunitySelectProps {
	companyId: string | null;
	/** An opportunity id, or "" for none. */
	value: string;
	onChange: (value: string) => void;
	errors: unknown[];
	isSubmitting?: boolean;
	fieldName: string;
	label?: string;
}

/**
 * The deals of one company, for a record that may say which deal it came from - a task, a project.
 * Only that company's: the API refuses a deal of another company anyway.
 */
export function OpportunitySelect({
	companyId,
	value,
	onChange,
	errors,
	isSubmitting,
	fieldName,
	label = "Opportunity",
}: OpportunitySelectProps) {
	const query = useGetOpportunitesSlice(
		{ companyId: companyId ?? undefined },
		{ pageSize: 50, enabled: Boolean(companyId) },
	);

	const deals = query.data?.pages.flatMap((page) => page.content) ?? [];

	return (
		<div className="form-field">
			<SelectField
				id={fieldName}
				name={fieldName}
				label={label}
				value={value}
				emptyLabel="No opportunity"
				isDisabled={isSubmitting || !companyId}
				onChange={onChange}
			>
				{deals.map((deal) => (
					<ListBox.Item key={deal.id} id={deal.id} textValue={deal.title}>
						{deal.title}
						<ListBox.ItemIndicator />
					</ListBox.Item>
				))}
			</SelectField>
			<FieldError errors={errors} />
		</div>
	);
}
