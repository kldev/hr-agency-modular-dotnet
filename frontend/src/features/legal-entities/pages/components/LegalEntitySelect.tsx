import { Select } from "@/components/ui";
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
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>

			<Select
				id={fieldName}
				name={fieldName}
				value={fieldValue ?? ""}
				disabled={isSubmitting || query.isPending || isEmpty}
				onChange={(event) => handleChange(event.target.value)}
			>
				<option value="">Select a company</option>

				{entities.map((entity) => (
					<option key={entity.id} value={entity.id}>
						{entity.name} · {entity.taxId}
					</option>
				))}
			</Select>

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
