import clsx from "clsx";
import { ArrowDown, ArrowUp, CircleAlert, Plus, Trash2 } from "lucide-react";
import { useState } from "react";
import type { SystemField } from "#/api/models";
import { Button, EmptyState } from "#/components/ui";
import { Input } from "#/components/ui/Input";
import { Select } from "#/components/ui/Select";
import { fieldTypes } from "../types";
import { FieldEditor } from "./FieldEditor";
import {
	type Layout,
	type LayoutAction,
	newFormField,
	newSystemField,
	suggestCode,
} from "./layout";

type BuildTabProps = {
	layout: Layout;
	dispatch: (action: LayoutAction) => void;
	catalogue: readonly SystemField[];
	codePrefix: string;
	/** What the last save or publication refused, keyed by page or field id. */
	errors: Record<string, string[]>;
};

/**
 * Pages on the left, the fields of the chosen page in the middle, the chosen field on the right.
 * Order changes by a step or by picking another page - drag and drop is a later stage.
 */
export function BuildTab({ layout, dispatch, catalogue, codePrefix, errors }: BuildTabProps) {
	const [pageId, setPageId] = useState<string | null>(layout[0]?.pageId ?? null);
	const [fieldId, setFieldId] = useState<string | null>(null);
	const [systemFieldId, setSystemFieldId] = useState("");

	const page = layout.find((candidate) => candidate.pageId === pageId) ?? layout[0];
	const field = layout.flatMap((candidate) => candidate.fields).find((f) => f.fieldId === fieldId);

	const used = new Set(layout.flatMap((candidate) => candidate.fields).map((f) => f.systemFieldId));
	const offered = catalogue.filter(
		(definition) => !definition.isArchived && !used.has(definition.systemFieldId),
	);

	const addPage = () => {
		const title = `Page ${layout.length + 1}`;
		dispatch({ type: "addPage", title });
	};

	const addFormField = () => {
		if (!page) return;

		const label = "New question";
		const taken = new Set(layout.flatMap((candidate) => candidate.fields).map((f) => f.code));
		let code = suggestCode(codePrefix, label);

		for (let n = 2; taken.has(code); n++) {
			code = `${suggestCode(codePrefix, label)}${n}`;
		}

		const created = { ...newFormField("Text", label, code) };
		dispatch({ type: "addField", pageId: page.pageId, field: created });
		setFieldId(created.fieldId);
	};

	const addSystemField = () => {
		const definition = catalogue.find((candidate) => candidate.systemFieldId === systemFieldId);

		if (!page || !definition) return;

		const created = newSystemField(definition);
		dispatch({ type: "addField", pageId: page.pageId, field: created });
		setFieldId(created.fieldId);
		setSystemFieldId("");
	};

	const general = errors[""] ?? [];

	return (
		<div className="flex flex-col gap-4">
			{general.length > 0 ? (
				<div className="form-error" role="alert">
					{general.map((error) => (
						<div key={error}>{error}</div>
					))}
				</div>
			) : null}

			<div className="grid gap-5 lg:grid-cols-[240px_minmax(0,1fr)_380px]">
				<section className="data-details-section" aria-label="Pages">
					<h3 className="mb-3 text-sm font-semibold">Pages</h3>

					<ul className="flex flex-col gap-1">
						{layout.map((candidate, index) => (
							<li key={candidate.pageId}>
								<button
									type="button"
									className={clsx(
										"flex w-full items-center gap-2 rounded-md px-2 py-1.5 text-left text-sm",
										candidate.pageId === page?.pageId
											? "bg-(--color-primary-soft) font-medium text-(--color-primary)"
											: "hover:bg-(--color-surface-hover)",
									)}
									onClick={() => {
										setPageId(candidate.pageId);
										setFieldId(null);
									}}
								>
									<span className="text-(--color-text-muted)">{index + 1}.</span>
									<span className="truncate">{candidate.title || "Untitled"}</span>
									<span className="ml-auto text-xs text-(--color-text-muted)">
										{candidate.fields.length}
									</span>
									{errors[candidate.pageId] ? (
										<CircleAlert size={14} className="text-(--color-danger)" />
									) : null}
								</button>
							</li>
						))}
					</ul>

					<Button variant="secondary" className="mt-3" icon={<Plus size={14} />} onClick={addPage}>
						Add page
					</Button>
				</section>

				<section className="data-details-section" aria-label="Fields">
					{page ? (
						<>
							<div className="mb-3 flex items-center gap-2">
								<Input
									aria-label="Page title"
									value={page.title}
									onChange={(event) =>
										dispatch({ type: "renamePage", pageId: page.pageId, title: event.target.value })
									}
								/>
								<Button
									variant="ghost"
									icon={<ArrowUp size={15} />}
									aria-label="Move page up"
									title="Move page up"
									onClick={() => dispatch({ type: "movePage", pageId: page.pageId, by: -1 })}
								/>
								<Button
									variant="ghost"
									icon={<ArrowDown size={15} />}
									aria-label="Move page down"
									title="Move page down"
									onClick={() => dispatch({ type: "movePage", pageId: page.pageId, by: 1 })}
								/>
								<Button
									variant="ghost"
									icon={<Trash2 size={15} />}
									aria-label="Remove page"
									title="Remove page"
									onClick={() => {
										dispatch({ type: "removePage", pageId: page.pageId });
										setPageId(null);
										setFieldId(null);
									}}
								/>
							</div>

							{(errors[page.pageId] ?? []).map((error) => (
								<p key={error} className="form-field-error mb-2">
									{error}
								</p>
							))}

							{page.fields.length === 0 ? (
								<EmptyState
									title="No fields on this page"
									description="Add a question of this form or a field from the catalogue."
								>
									<Plus size={24} />
								</EmptyState>
							) : (
								<ul className="flex flex-col gap-1">
									{page.fields.map((candidate) => (
										<li key={candidate.fieldId}>
											<button
												type="button"
												className={clsx(
													"flex w-full items-center gap-3 rounded-md border px-3 py-2 text-left text-sm",
													candidate.fieldId === fieldId
														? "border-(--color-primary) bg-(--color-primary-soft)"
														: "border-(--color-border) hover:bg-(--color-surface-hover)",
												)}
												onClick={() => setFieldId(candidate.fieldId)}
											>
												<span className="min-w-0 flex-1">
													<span className="block truncate font-medium">
														{candidate.labelOverride ?? candidate.label}
														{candidate.rules.required ? " *" : ""}
													</span>
													<span className="block truncate text-xs text-(--color-text-muted)">
														{candidate.code} · {fieldTypes[candidate.type]}
													</span>
												</span>
												{candidate.source === "System" ? (
													<span className="badge badge-viewed">system</span>
												) : null}
												{errors[candidate.fieldId] ? (
													<CircleAlert size={15} className="text-(--color-danger)" />
												) : null}
											</button>
										</li>
									))}
								</ul>
							)}

							<div className="mt-4 flex flex-wrap items-center gap-2">
								<Button variant="secondary" icon={<Plus size={14} />} onClick={addFormField}>
									Add question
								</Button>

								<Select
									aria-label="System field to add"
									className="max-w-64"
									value={systemFieldId}
									onChange={(event) => setSystemFieldId(event.target.value)}
								>
									<option value="">System field…</option>
									{offered.map((definition) => (
										<option key={definition.systemFieldId} value={definition.systemFieldId}>
											{definition.label} ({definition.code})
										</option>
									))}
								</Select>

								<Button variant="secondary" disabled={!systemFieldId} onClick={addSystemField}>
									Add system field
								</Button>
							</div>
						</>
					) : (
						<EmptyState
							title="No pages yet"
							description="A form with one page is a plain form; several make a wizard."
						>
							<Plus size={24} />
						</EmptyState>
					)}
				</section>

				<section className="data-details-section" aria-label="Field settings">
					{field ? (
						<FieldEditor
							key={field.fieldId}
							field={field}
							pages={layout}
							codePrefix={codePrefix}
							errors={errors[field.fieldId] ?? []}
							onChange={(changed) => dispatch({ type: "updateField", field: changed })}
							onMove={(by) => dispatch({ type: "moveField", fieldId: field.fieldId, by })}
							onMoveToPage={(target) => {
								dispatch({ type: "moveFieldToPage", fieldId: field.fieldId, pageId: target });
								setPageId(target);
							}}
							onRemove={() => {
								dispatch({ type: "removeField", fieldId: field.fieldId });
								setFieldId(null);
							}}
						/>
					) : (
						<p className="form-hint">Pick a field to change its label, type, rules and options.</p>
					)}
				</section>
			</div>
		</div>
	);
}
