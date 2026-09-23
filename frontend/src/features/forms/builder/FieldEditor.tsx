import { ArrowDown, ArrowUp, Trash2 } from "lucide-react";
import type { FieldType, FormField, FormPage } from "#/api/models";
import { Button, EnumSelectFilter } from "#/components/ui";
import { Input } from "#/components/ui/Input";
import { Select } from "#/components/ui/Select";
import { Textarea } from "#/components/ui/Textarea";
import { OptionsEditor } from "../components/OptionsEditor";
import { RulesEditor } from "../components/RulesEditor";
import { fieldTypes, hasOptions, takesTypedText } from "../types";
import { suggestCode } from "./layout";

type FieldEditorProps = {
	field: FormField;
	pages: readonly FormPage[];
	codePrefix: string;
	errors: string[];
	onChange: (field: FormField) => void;
	onMove: (by: -1 | 1) => void;
	onMoveToPage: (pageId: string) => void;
	onRemove: () => void;
};

/**
 * Everything about one field. A system field shows its catalogue definition and lets the form rename
 * it and give it a placeholder - its type and rules are the catalogue's, and the backend fills them
 * in from there on every save whatever this panel sends.
 */
export function FieldEditor({
	field,
	pages,
	codePrefix,
	errors,
	onChange,
	onMove,
	onMoveToPage,
	onRemove,
}: FieldEditorProps) {
	const system = field.source === "System";
	const id = `field-${field.fieldId}`;
	const set = (patch: Partial<FormField>) => onChange({ ...field, ...patch });

	const changeLabel = (label: string) => {
		// The code follows the label for as long as nobody has written one of their own.
		const following = field.code === suggestCode(codePrefix, field.label);

		set({ label, ...(following ? { code: suggestCode(codePrefix, label) } : {}) });
	};

	const changeType = (type: FieldType) =>
		set({
			type,
			// Rules of the old type would be refused for the new one; only "required" carries over.
			rules: { ...emptyRules, required: field.rules.required, message: field.rules.message },
			options: hasOptions(type)
				? field.options.length
					? field.options
					: [{ value: "yes", label: "Yes" }]
				: [],
		});

	const page = pages.find((candidate) => candidate.fields.some((f) => f.fieldId === field.fieldId));

	return (
		<aside className="flex flex-col gap-4">
			<div className="flex items-center justify-between gap-2">
				<h3 className="text-base font-semibold">{system ? "System field" : "Form field"}</h3>

				<div className="flex gap-1">
					<Button
						variant="ghost"
						icon={<ArrowUp size={15} />}
						aria-label="Move up"
						title="Move up"
						onClick={() => onMove(-1)}
					/>
					<Button
						variant="ghost"
						icon={<ArrowDown size={15} />}
						aria-label="Move down"
						title="Move down"
						onClick={() => onMove(1)}
					/>
					<Button
						variant="ghost"
						icon={<Trash2 size={15} />}
						aria-label="Remove field"
						title="Remove field"
						onClick={onRemove}
					/>
				</div>
			</div>

			{errors.length > 0 ? (
				<div className="form-error" role="alert">
					{errors.map((error) => (
						<div key={error}>{error}</div>
					))}
				</div>
			) : null}

			{pages.length > 1 ? (
				<label className="form-field" htmlFor={`${id}-page`}>
					<span className="form-label">Page</span>
					<Select
						id={`${id}-page`}
						value={page?.pageId ?? ""}
						onChange={(event) => onMoveToPage(event.target.value)}
					>
						{pages.map((candidate) => (
							<option key={candidate.pageId} value={candidate.pageId}>
								{candidate.title}
							</option>
						))}
					</Select>
				</label>
			) : null}

			{system ? (
				<>
					<p className="form-hint">
						<strong>{field.code}</strong> · {fieldTypes[field.type]}. Its type and rules come from
						the catalogue; a published version keeps the ones it went out with.
					</p>

					<label className="form-field" htmlFor={`${id}-label`}>
						<span className="form-label">Label on this form (empty = catalogue label)</span>
						<Input
							id={`${id}-label`}
							placeholder={field.label}
							value={field.labelOverride ?? ""}
							onChange={(event) => set({ labelOverride: event.target.value || null })}
						/>
					</label>
				</>
			) : (
				<>
					<label className="form-field" htmlFor={`${id}-label`}>
						<span className="form-label">Label *</span>
						<Input
							id={`${id}-label`}
							value={field.label}
							onChange={(event) => changeLabel(event.target.value)}
						/>
					</label>

					<label className="form-field" htmlFor={`${id}-code`}>
						<span className="form-label">Code * (what reports ask by, e.g. gdpr.consent)</span>
						<Input
							id={`${id}-code`}
							value={field.code}
							onChange={(event) => set({ code: event.target.value })}
						/>
					</label>

					<div className="form-field">
						<span className="form-label">Type</span>
						<EnumSelectFilter
							id={`${id}-type`}
							hideAll={true}
							value={field.type}
							options={fieldTypes}
							onChange={(value) => value && changeType(value as FieldType)}
						/>
					</div>

					<label className="form-field" htmlFor={`${id}-description`}>
						<span className="form-label">Help text</span>
						<Textarea
							id={`${id}-description`}
							rows={2}
							value={field.description ?? ""}
							onChange={(event) => set({ description: event.target.value || null })}
						/>
					</label>
				</>
			)}

			{takesTypedText(field.type) || field.type === "Number" ? (
				<label className="form-field" htmlFor={`${id}-placeholder`}>
					<span className="form-label">Placeholder</span>
					<Input
						id={`${id}-placeholder`}
						value={field.placeholder ?? ""}
						onChange={(event) => set({ placeholder: event.target.value || null })}
					/>
				</label>
			) : null}

			{hasOptions(field.type) ? (
				<OptionsEditor
					idPrefix={id}
					options={field.options}
					disabled={system}
					onChange={(options) => set({ options })}
				/>
			) : null}

			<RulesEditor
				idPrefix={id}
				type={field.type}
				rules={field.rules}
				disabled={system}
				onChange={(rules) => set({ rules })}
			/>
		</aside>
	);
}

const emptyRules: FormField["rules"] = {
	required: false,
	minLength: null,
	maxLength: null,
	pattern: null,
	min: null,
	max: null,
	decimals: null,
	minDate: null,
	maxDate: null,
	minSelected: null,
	maxSelected: null,
	message: null,
};
