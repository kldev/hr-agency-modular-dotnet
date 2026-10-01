import { ListBox } from "@heroui/react";
import { SelectField } from "@/components/ui";
import { useActiveLegalEntities } from "../hooks";

interface LegalEntitySelectProps {
	label: string;
	fieldName: string;
	fieldValue: string | null;
	errors: Array<unknown>;
	isSubmitting?: boolean;
	handleChange: (value: string) => void;
}

/**
 * Only companies still trading are offered: a project cannot be started by one that has been wound
 * up, and the backend refuses it anyway. A closed entity keeps the projects it already runs.
 */
export function LegalEntitySelect({
	label,
	fieldName,
	fieldValue,
	errors,
	isSubmitting,
	handleChange,
}: LegalEntitySelectProps) {
	const query = useActiveLegalEntities();

	const entities = query.data?.content ?? [];
	const isEmpty = query.isFetched && entities.length === 0;

	return (
		<div className="form-field">
			<SelectField
				id={fieldName}
				name={fieldName}
				label={label}
				value={fieldValue}
				placeholder="Select a company"
				isDisabled={isSubmitting || query.isPending || isEmpty}
				onChange={handleChange}
			>
				{entities.map((entity) => (
					<ListBox.Item
						key={entity.id}
						id={entity.id}
						textValue={`${entity.name} · ${entity.taxId}`}
					>
						{entity.name} · {entity.taxId}
						<ListBox.ItemIndicator />
					</ListBox.Item>
				))}
			</SelectField>

			{isEmpty ? (
				<p className="form-error">
					There is no trading legal entity yet. Add one before setting up a project.
				</p>
			) : null}

			{errors.length > 0 ? (
				<p className="form-error">{String((errors[0] as { message?: string })?.message ?? "")}</p>
			) : null}
		</div>
	);
}
