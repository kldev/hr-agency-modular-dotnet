import type { FieldRules, FieldType } from "#/api/models";
import { Input } from "#/components/ui/Input";
import { Toggle } from "#/components/ui/Toggle";
import { takesTypedText } from "../types";

type RulesEditorProps = {
	idPrefix: string;
	type: FieldType;
	rules: FieldRules;
	disabled?: boolean;
	onChange: (rules: FieldRules) => void;
};

const toNumber = (value: string) => (value.trim() === "" ? null : Number(value.replace(",", ".")));

/**
 * The rules a type can use, and only those - mirrors the applicability checks of
 * `FieldRulesPolicy`, which refuses any other. A rule nobody can set is a rule nobody wonders about.
 */
export function RulesEditor({ idPrefix, type, rules, disabled, onChange }: RulesEditorProps) {
	const set = (patch: Partial<FieldRules>) => onChange({ ...rules, ...patch });

	const number = (key: keyof FieldRules, label: string, step = "1") => (
		<label className="form-field" htmlFor={`${idPrefix}-${key}`}>
			<span className="form-label">{label}</span>
			<Input
				id={`${idPrefix}-${key}`}
				type="number"
				step={step}
				disabled={disabled}
				value={rules[key] == null ? "" : String(rules[key])}
				onChange={(event) => set({ [key]: toNumber(event.target.value) })}
			/>
		</label>
	);

	const date = (key: "minDate" | "maxDate", label: string) => (
		<label className="form-field" htmlFor={`${idPrefix}-${key}`}>
			<span className="form-label">{label}</span>
			<Input
				id={`${idPrefix}-${key}`}
				type="date"
				disabled={disabled}
				value={rules[key] ?? ""}
				onChange={(event) => set({ [key]: event.target.value || null })}
			/>
		</label>
	);

	return (
		<div className="flex flex-col gap-3">
			<span className="toolbar-toggle">
				<Toggle
					id={`${idPrefix}-required`}
					checked={rules.required === true}
					disabled={disabled}
					onChange={(event) => set({ required: event.target.checked })}
				/>
				<label htmlFor={`${idPrefix}-required`}>
					{type === "Boolean" ? "Must be ticked" : "Required"}
				</label>
			</span>

			<div className="grid grid-cols-2 gap-3">
				{takesTypedText(type) ? (
					<>
						{number("minLength", "Min. length")}
						{number("maxLength", "Max. length")}
					</>
				) : null}

				{type === "Number" ? (
					<>
						{number("min", "Minimum", "any")}
						{number("max", "Maximum", "any")}
						{number("decimals", "Decimal places")}
					</>
				) : null}

				{type === "Date" ? (
					<>
						{date("minDate", "Earliest date")}
						{date("maxDate", "Latest date")}
					</>
				) : null}

				{type === "MultiChoice" ? (
					<>
						{number("minSelected", "Min. selected")}
						{number("maxSelected", "Max. selected")}
					</>
				) : null}
			</div>

			{takesTypedText(type) ? (
				<label className="form-field" htmlFor={`${idPrefix}-pattern`}>
					<span className="form-label">Pattern (regular expression, matches the whole value)</span>
					<Input
						id={`${idPrefix}-pattern`}
						placeholder="\d{11}"
						disabled={disabled}
						value={rules.pattern ?? ""}
						onChange={(event) => set({ pattern: event.target.value || null })}
					/>
				</label>
			) : null}

			<label className="form-field" htmlFor={`${idPrefix}-message`}>
				<span className="form-label">Own error message (optional)</span>
				<Input
					id={`${idPrefix}-message`}
					placeholder="Enter a valid PESEL number"
					disabled={disabled}
					value={rules.message ?? ""}
					onChange={(event) => set({ message: event.target.value || null })}
				/>
			</label>
		</div>
	);
}
