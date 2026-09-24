import { RefreshCcw } from "lucide-react";
import { useEffect } from "react";
import { toast } from "sonner";
import type { FormPage } from "#/api/models";
import { Button, EmptyState } from "#/components/ui";
import { usePreviewFormLayout } from "../hooks";
import { DynamicForm } from "../renderer/DynamicForm";

/**
 * The form exactly as it would be published - system fields resolved by the backend from today's
 * catalogue - drawn by the same renderer people fill it in with. Nothing is saved: neither the
 * layout, which may still be unsaved, nor the answers typed to try it.
 */
export function PreviewTab({ formId, layout }: { formId: string; layout: readonly FormPage[] }) {
	const preview = usePreviewFormLayout();
	const { mutate } = preview;

	useEffect(() => {
		mutate({ formId, req: { pages: [...layout] } });
	}, [formId, layout, mutate]);

	const resolved = preview.data;

	return (
		<div className="flex flex-col gap-4">
			<div className="flex items-center justify-between gap-3">
				<p className="form-hint">
					Try the form as it will be filled in. Answers typed here are thrown away.
				</p>
				<Button
					variant="secondary"
					icon={<RefreshCcw size={14} />}
					loading={preview.isPending}
					onClick={() => mutate({ formId, req: { pages: [...layout] } })}
				>
					Refresh preview
				</Button>
			</div>

			{resolved && resolved.errors.length > 0 ? (
				<div className="form-error" role="alert">
					<strong>Publishing would be refused:</strong>
					<ul className="form-wizard__summary-list">
						{resolved.errors.map((error) => (
							<li key={error}>{error}</li>
						))}
					</ul>
				</div>
			) : null}

			{resolved && resolved.pages.length > 0 ? (
				<div className="data-details-section h-[70vh] overflow-auto p-4">
					<DynamicForm
						key={JSON.stringify(resolved.pages)}
						pages={resolved.pages}
						submitLabel="Check answers"
						onSubmit={() =>
							toast.success("Every answer passes the form's rules. Nothing was saved.")
						}
					/>
				</div>
			) : resolved ? (
				<EmptyState title="Nothing to preview" description="Add a page with a field first.">
					<RefreshCcw size={24} />
				</EmptyState>
			) : null}
		</div>
	);
}
