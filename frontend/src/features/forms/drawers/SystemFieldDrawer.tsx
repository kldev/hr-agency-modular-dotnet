import { forwardRef, useImperativeHandle, useState } from "react";
import type {
	BadRequestDetails,
	ChoiceOption,
	FieldRules,
	FieldType,
	SystemField,
	SystemFieldSource,
} from "#/api/models";
import { Button, EnumSelectFilter } from "#/components/ui";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { Input } from "#/components/ui/Input";
import { Textarea } from "#/components/ui/Textarea";
import { OptionsEditor } from "../components/OptionsEditor";
import { RulesEditor } from "../components/RulesEditor";
import { useDefineSystemField, useUpdateSystemField } from "../hooks";
import {
	fieldTypes,
	hasOptions,
	sourceTypes,
	systemCodePrefix,
	systemFieldSources,
} from "../types";

export interface SystemFieldCommand {
	define: () => void;
	edit: (field: SystemField) => void;
}

type Draft = {
	code: string;
	type: FieldType;
	label: string;
	description: string;
	rules: FieldRules;
	options: ChoiceOption[];
	source: SystemFieldSource;
};

const emptyDraft: Draft = {
	code: systemCodePrefix,
	type: "Text",
	label: "",
	description: "",
	rules: { required: false },
	options: [],
	source: "None",
};

/**
 * One entry of the catalogue. Code and type are asked once and never again: changing either would
 * change what every value already given for the field means, so that is a new field, not an edit.
 */
function FormContent({
	editing,
	onSaved,
	handleClose,
}: {
	editing: SystemField | null;
	onSaved: () => void;
	handleClose: () => void;
}) {
	const [draft, setDraft] = useState<Draft>(
		editing
			? {
					code: editing.code,
					type: editing.type,
					label: editing.label,
					description: editing.description ?? "",
					rules: editing.rules,
					options: editing.options,
					source: editing.source,
				}
			: emptyDraft,
	);

	const done = () => {
		onSaved();
		handleClose();
	};

	const define = useDefineSystemField({ onSuccess: done });
	const update = useUpdateSystemField({ onSuccess: done });
	const mutation = editing ? update.mutation : define.mutation;

	const set = (patch: Partial<Draft>) => setDraft((current) => ({ ...current, ...patch }));

	/* A source fits only a field of the type it gives - mirrors `SystemFieldSources.ValueType`. */
	const sources = Object.fromEntries(
		(Object.keys(systemFieldSources) as SystemFieldSource[])
			.filter((source) => source === "None" || sourceTypes[source] === draft.type)
			.map((source) => [source, systemFieldSources[source]]),
	) as Record<SystemFieldSource, string>;

	const submit = () => {
		const common = {
			label: draft.label,
			description: draft.description || null,
			rules: draft.rules,
			options: hasOptions(draft.type) ? draft.options : [],
			source: draft.source,
		};

		if (editing) {
			update.mutation.mutate({ id: editing.systemFieldId, req: common });
		} else {
			define.mutation.mutate({ ...common, code: draft.code, type: draft.type });
		}
	};

	return (
		<FormDrawer
			open={true}
			title={editing ? "Edit system field" : "New system field"}
			onClose={handleClose}
			onSubmit={(event) => {
				event.preventDefault();
				submit();
			}}
		>
			<FormDrawer.Content>
				<div className="drawer-form">
					<label className="form-field" htmlFor="system-field-code">
						<span className="form-label">Code (never changes)</span>
						<Input
							id="system-field-code"
							disabled={Boolean(editing)}
							value={draft.code}
							onChange={(event) => set({ code: event.target.value })}
						/>
					</label>

					<div className="form-field">
						<span className="form-label">Type (never changes)</span>
						{editing ? (
							<p className="form-hint">{fieldTypes[draft.type]}</p>
						) : (
							<EnumSelectFilter
								id="system-field-type"
								hideAll={true}
								value={draft.type}
								options={fieldTypes}
								onChange={(value) =>
									value &&
									set({
										type: value as FieldType,
										rules: { required: draft.rules.required },
										source: "None",
									})
								}
							/>
						)}
					</div>

					<label className="form-field" htmlFor="system-field-label">
						<span className="form-label">Label</span>
						<Input
							id="system-field-label"
							value={draft.label}
							onChange={(event) => set({ label: event.target.value })}
						/>
					</label>

					<label className="form-field" htmlFor="system-field-description">
						<span className="form-label">Description</span>
						<Textarea
							id="system-field-description"
							rows={2}
							value={draft.description}
							onChange={(event) => set({ description: event.target.value })}
						/>
					</label>

					<div className="form-field">
						<span className="form-label">Pre-filled from</span>
						<EnumSelectFilter
							id="system-field-source"
							hideAll={true}
							value={draft.source}
							options={sources}
							onChange={(value) => set({ source: (value as SystemFieldSource) ?? "None" })}
						/>
					</div>

					{hasOptions(draft.type) ? (
						<OptionsEditor
							idPrefix="system-field"
							options={draft.options}
							onChange={(options) => set({ options })}
						/>
					) : null}

					<RulesEditor
						idPrefix="system-field"
						type={draft.type}
						rules={draft.rules}
						onChange={(rules) => set({ rules })}
					/>

					<ApiError error={mutation.error as unknown as BadRequestDetails | null} />
				</div>
			</FormDrawer.Content>

			<FormDrawer.Footer>
				<Button variant="primary" type="submit" loading={mutation.isPending}>
					Save changes
				</Button>
			</FormDrawer.Footer>
		</FormDrawer>
	);
}

const SystemFieldDrawer = forwardRef<SystemFieldCommand, { onSaved: () => void }>(
	({ onSaved }, ref) => {
		const [target, setTarget] = useState<{ editing: SystemField | null } | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				define: () => setTarget({ editing: null }),
				edit: (field) => setTarget({ editing: field }),
			}),
			[],
		);

		if (!target) return null;

		return (
			<FormContent editing={target.editing} onSaved={onSaved} handleClose={() => setTarget(null)} />
		);
	},
);

SystemFieldDrawer.displayName = "SystemFieldDrawer";

export { SystemFieldDrawer };
