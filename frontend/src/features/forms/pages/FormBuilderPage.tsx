import { Archive, Save, Send } from "lucide-react";
import { useEffect, useReducer, useState } from "react";
import { toast } from "sonner";
import type { BadRequestDetails, FormDefinitionView } from "#/api/models";
import { Page } from "#/components/layout";
import {
	Button,
	ConfirmDialog,
	DetailsLoading,
	FormStatusBadge,
	type TabDefinition,
	TabPanel,
	Tabs,
} from "#/components/ui";
import { BuildTab } from "../builder/BuildTab";
import { codePrefix, type Layout, layoutReducer } from "../builder/layout";
import { PreviewTab } from "../builder/PreviewTab";
import { VersionsTab } from "../builder/VersionsTab";
import {
	useArchiveForm,
	useGetForm,
	useGetSystemFields,
	usePublishForm,
	useSaveFormDraft,
} from "../hooks";
import { fieldErrorsOf } from "../schema/fieldErrors";
import { cardinalities, formKinds } from "../types";

export type BuilderTab = "build" | "preview" | "versions";

export const builderTabs: readonly BuilderTab[] = ["build", "preview", "versions"];

type FormBuilderPageProps = {
	formId: string;
	tab: BuilderTab;
	onTabChange: (tab: BuilderTab) => void;
};

/**
 * The form builder, on a route of its own rather than in a dialog (plan 028): it is a workspace with
 * a preview, not a wizard somebody walks through once.
 *
 * The layout is edited locally and saved whole (plan 028 §3.3). The preview renders the local copy,
 * so what is on screen can be tried before it is saved; publishing saves first when there is
 * anything unsaved, because a version is made from the stored draft.
 */
function Builder({
	form,
	tab,
	onTabChange,
}: { form: FormDefinitionView } & Omit<FormBuilderPageProps, "formId">) {
	const catalogue = useGetSystemFields();
	const [layout, dispatch] = useReducer(layoutReducer, form.pages as Layout);
	const [archiving, setArchiving] = useState(false);

	/* The saved draft comes back with system fields filled in; take it as the new starting point. */
	useEffect(() => {
		dispatch({ type: "reset", layout: form.pages as Layout });
	}, [form.pages]);

	const dirty = JSON.stringify(layout) !== JSON.stringify(form.pages);
	const archived = form.status === "Archived";

	const save = useSaveFormDraft({ onSuccess: () => toast.success("Draft saved") });
	const publish = usePublishForm({ onSuccess: () => toast.success("Published") });
	const archive = useArchiveForm({
		onSuccess: () => {
			setArchiving(false);
			toast.success("Form archived");
		},
	});

	const saveDraft = () => save.mutation.mutateAsync({ formId: form.id, req: { pages: layout } });

	const onPublish = async () => {
		if (dirty) {
			await saveDraft();
		}

		publish.mutation.mutate(form.id);
	};

	const errors = fieldErrorsOf(publish.mutation.error ?? save.mutation.error) ?? {};
	const refusal = (publish.mutation.error ?? save.mutation.error) as BadRequestDetails | null;

	const tabs: TabDefinition<BuilderTab>[] = [
		{ id: "build", label: "Build" },
		{ id: "preview", label: "Preview" },
		{ id: "versions", label: "Versions", count: form.versions.length },
	];

	return (
		<>
			<Page
				title={form.name}
				description={`${formKinds[form.kind]} · ${cardinalities[form.cardinality]} · code ${form.code}`}
				emptyState={null}
				wide
				headerAddon={
					<div className="flex flex-wrap items-center gap-2">
						<FormStatusBadge status={form.status} />
						{Number(form.publishedVersion) > 0 ? (
							<span className="badge badge-inactive">v{String(form.publishedVersion)}</span>
						) : null}
						{dirty ? <span className="badge badge-suspended">Unsaved changes</span> : null}
						{!dirty && form.hasUnpublishedChanges && form.status === "Published" ? (
							<span className="badge badge-viewed">
								Draft ahead of v{String(form.publishedVersion)}
							</span>
						) : null}

						{archived ? null : (
							<>
								<Button
									variant="secondary"
									icon={<Save size={14} />}
									disabled={!dirty}
									loading={save.mutation.isPending}
									onClick={() => void saveDraft().catch(() => undefined)}
								>
									Save draft
								</Button>
								<Button
									variant="primary"
									icon={<Send size={14} />}
									loading={publish.mutation.isPending || publish.waiting}
									disabled={
										!dirty && !form.hasUnpublishedChanges && Number(form.publishedVersion) > 0
									}
									onClick={() => void onPublish().catch(() => undefined)}
								>
									Publish
								</Button>
								<Button
									variant="ghost"
									icon={<Archive size={14} />}
									onClick={() => setArchiving(true)}
								>
									Archive
								</Button>
							</>
						)}
					</div>
				}
			>
				{refusal && !Object.keys(errors).length ? (
					<div className="form-error mb-4" role="alert">
						{refusal.detail ?? refusal.title}
					</div>
				) : null}

				<Tabs value={tab} tabs={tabs} onChange={onTabChange} label="Builder sections" />

				<TabPanel id={tab}>
					{tab === "build" ? (
						archived ? (
							<p className="form-hint">
								This form is archived; its versions stay readable under Versions.
							</p>
						) : (
							<BuildTab
								layout={layout}
								dispatch={dispatch}
								catalogue={catalogue.data ?? []}
								codePrefix={codePrefix(form.code)}
								errors={errors}
							/>
						)
					) : null}

					{tab === "preview" ? <PreviewTab formId={form.id} layout={layout} /> : null}

					{tab === "versions" ? <VersionsTab formId={form.id} versions={form.versions} /> : null}
				</TabPanel>
			</Page>

			<ConfirmDialog
				open={archiving}
				title="Archive this form?"
				description="Nobody can start it any more. Responses already given - and drafts already started - stay as they are."
				confirmLabel="Archive"
				onConfirm={() => archive.mutation.mutate(form.id)}
				onClose={() => setArchiving(false)}
			/>
		</>
	);
}

export default function FormBuilderPage({ formId, tab, onTabChange }: FormBuilderPageProps) {
	const query = useGetForm(formId);

	if (query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading
				id={formId}
				isLoading={query.isLoading}
				isError={query.isError || !query.data}
			/>
		);
	}

	return <Builder form={query.data} tab={tab} onTabChange={onTabChange} />;
}
