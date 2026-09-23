import { Link, useNavigate } from "@tanstack/react-router";
import { ClipboardList, ListChecks, Plus, Search } from "lucide-react";
import { useRef } from "react";
import type { FormKind } from "#/api/models";
import { Page } from "#/components/layout";
import { Button, EmptyState, EnumFilter, FormStatusBadge, LoadMore } from "#/components/ui";
import { Input } from "#/components/ui/Input";
import { useAuthStore } from "#/stores/authStore";
import { formatDateTime } from "#/utlis/dateUtils";
import { type CreateFormCommand, CreateFormDrawer } from "../drawers/CreateFormDrawer";
import { useGetFormsSlice } from "../hooks";
import { formKinds, isFormsDesigner } from "../types";

type FormsPageProps = {
	search: string;
	kind: FormKind | null;
	onSearchChange: (value: string) => void;
	onKindChange: (value: FormKind | null) => void;
};

/**
 * Every form, document and survey of the agency. Open to everybody, since the people who fill forms
 * in pick from these; building them is for `FormsDesignPolicy` only, and the page shows its buttons
 * to those people alone.
 */
export default function FormsPage({ search, kind, onSearchChange, onKindChange }: FormsPageProps) {
	const role = useAuthStore((state) => state.user?.role);
	const designer = isFormsDesigner(role);
	const createRef = useRef<CreateFormCommand>(null);
	const navigate = useNavigate();

	const query = useGetFormsSlice({ search, kind: kind ? [kind] : undefined });
	const forms = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.at(-1)?.hasMore ?? false;

	return (
		<>
			<Page
				title="Forms"
				description="Documents, consents and surveys the agency defines itself - no developer needed for a new one."
				onRefresh={() => void query.refetch()}
				loading={query.isPending}
				isEmpty={query.isFetched && forms.length === 0 && !search && !kind}
				emptyState={
					<EmptyState
						title="No forms yet"
						description="Create the first one - a consent, a statement or a survey."
					>
						<ClipboardList size={24} />
					</EmptyState>
				}
			>
				<div className="toolbar">
					<div className="toolbar-left">
						<span className="search-input">
							<Search
								size={15}
								aria-hidden="true"
								style={{ position: "absolute", left: 12, top: 10 }}
							/>
							<span className="sr-only">Search forms</span>
							<Input
								value={search}
								placeholder="Search forms"
								onChange={(event) => onSearchChange(event.target.value)}
							/>
						</span>

						<EnumFilter
							value={kind}
							options={formKinds}
							onChange={(value) => onKindChange((value as FormKind) ?? null)}
						/>
					</div>

					{designer ? (
						<div className="toolbar-right flex gap-2">
							<Link to="/app/forms/system-fields">
								<Button variant="secondary" icon={<ListChecks size={15} />}>
									System fields
								</Button>
							</Link>
							<Button
								variant="primary"
								icon={<Plus size={15} />}
								onClick={() => createRef.current?.create()}
							>
								New form
							</Button>
						</div>
					) : null}
				</div>

				<table className="table">
					<thead>
						<tr>
							<th>Name</th>
							<th className="table-header-sm">Kind</th>
							<th className="table-header-sm">Status</th>
							<th className="table-header-sm">Version</th>
							<th className="table-header-sm">Fields</th>
							<th>Last change</th>
						</tr>
					</thead>
					<tbody>
						{forms.map((form) => (
							<tr key={form.id}>
								<td>
									{designer ? (
										<Link
											to="/app/forms/$formId"
											params={{ formId: form.id }}
											search={{ tab: undefined }}
											className="font-medium"
										>
											{form.name}
										</Link>
									) : (
										form.name
									)}
									<div className="text-xs text-(--color-text-muted)">{form.code}</div>
								</td>
								<td>{formKinds[form.kind]}</td>
								<td>
									<FormStatusBadge status={form.status} />
								</td>
								<td className="table-figure">
									{Number(form.publishedVersion) > 0 ? `v${String(form.publishedVersion)}` : "—"}
									{form.hasUnpublishedChanges && form.status === "Published"
										? " · draft ahead"
										: ""}
								</td>
								<td className="table-figure">
									{String(form.fieldCount)} on {String(form.pageCount)}{" "}
									{Number(form.pageCount) === 1 ? "page" : "pages"}
								</td>
								<td>{formatDateTime(form.modifiedAt ?? form.createdAt)}</td>
							</tr>
						))}
					</tbody>
				</table>

				<LoadMore
					loading={query.isFetchingNextPage}
					hasNext={hasMore}
					onClick={() => void query.fetchNextPage()}
				/>
			</Page>

			<CreateFormDrawer
				ref={createRef}
				onCreated={(created) =>
					navigate({
						to: "/app/forms/$formId",
						params: { formId: created.formId },
						search: { tab: undefined },
					})
				}
			/>
		</>
	);
}
