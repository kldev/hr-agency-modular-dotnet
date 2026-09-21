import clsx from "clsx";
import { ChevronDown, ChevronRight } from "lucide-react";
import { useState } from "react";
import type { UserProjection } from "@/api/models";
import { type OrgUnitNode, orgUnitKinds } from "../../types";

interface OrgUnitTreeProps {
	nodes: OrgUnitNode[];
	selectedId: string | null;
	onSelect: (unitId: string) => void;
	resolveUser: (userId: string) => UserProjection | undefined;
}

/**
 * The chart as the tree it is. Recursive rather than a table: `MainTable` is built for flat, paged
 * rows, and bolting a parent chain onto it costs more than the twenty lines of recursion below -
 * for a chart that is tens of boxes and is never paged.
 *
 * Collapsed rather than expanded state is tracked, so a chart arrives fully open without anybody
 * having to seed the set from the data first.
 */
export function OrgUnitTree({ nodes, selectedId, onSelect, resolveUser }: OrgUnitTreeProps) {
	const [collapsed, setCollapsed] = useState<Set<string>>(new Set());

	const toggle = (unitId: string) =>
		setCollapsed((previous) => {
			const next = new Set(previous);

			if (!next.delete(unitId)) {
				next.add(unitId);
			}

			return next;
		});

	return (
		<ul className="org-tree">
			{nodes.map((node) => (
				<OrgUnitTreeBranch
					key={node.unitId}
					node={node}
					collapsed={collapsed}
					selectedId={selectedId}
					onToggle={toggle}
					onSelect={onSelect}
					resolveUser={resolveUser}
				/>
			))}
		</ul>
	);
}

interface BranchProps {
	node: OrgUnitNode;
	collapsed: Set<string>;
	selectedId: string | null;
	onToggle: (unitId: string) => void;
	onSelect: (unitId: string) => void;
	resolveUser: (userId: string) => UserProjection | undefined;
}

function OrgUnitTreeBranch({
	node,
	collapsed,
	selectedId,
	onToggle,
	onSelect,
	resolveUser,
}: BranchProps) {
	const hasChildren = node.children.length > 0;
	const isOpen = hasChildren && !collapsed.has(node.unitId);

	const head = node.headUserId ? resolveUser(node.headUserId) : undefined;

	return (
		<li>
			<div
				className={clsx("org-tree-row", selectedId === node.unitId && "org-tree-row-selected")}
				style={{ paddingLeft: `calc(var(--space-3) + ${node.depth} * var(--space-5))` }}
			>
				{hasChildren ? (
					<button
						type="button"
						className="org-tree-toggle"
						aria-expanded={isOpen}
						aria-label={isOpen ? `Collapse ${node.name}` : `Expand ${node.name}`}
						onClick={() => onToggle(node.unitId)}
					>
						{isOpen ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
					</button>
				) : (
					<span className="org-tree-toggle-spacer" aria-hidden="true" />
				)}

				<button type="button" className="org-tree-name" onClick={() => onSelect(node.unitId)}>
					<span className="org-tree-label">
						{node.name}

						{node.isArchived ? <span className="org-tree-archived">Archived</span> : null}
					</span>

					<span className="org-tree-meta">
						{orgUnitKinds[node.kind]}
						{" · "}
						{/*
						 * A unit with no head is a normal shape, not a gap to fill: the head of the unit
						 * above answers for it. Saying so beats an empty space that reads as missing data.
						 */}
						{head ? (head.fullName ?? head.email) : "led from above"}
					</span>
				</button>
			</div>

			{isOpen ? (
				<ul>
					{node.children.map((child) => (
						<OrgUnitTreeBranch
							key={child.unitId}
							node={child}
							collapsed={collapsed}
							selectedId={selectedId}
							onToggle={onToggle}
							onSelect={onSelect}
							resolveUser={resolveUser}
						/>
					))}
				</ul>
			) : null}
		</li>
	);
}
