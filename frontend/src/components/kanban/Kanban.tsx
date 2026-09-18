import clsx from "clsx";
import type { ReactNode } from "react";
import "./kanban.css";

type KanbanProps = {
	children: ReactNode;
	label: string;
	className?: string;
};

type KanbanColumnProps = {
	children: ReactNode;
	label: string;
	className?: string;
};

type KanbanColumnHeaderProps = {
	title: string;
	icon?: ReactNode;
	count?: number;
	meta?: ReactNode;
	className?: string;
};

type KanbanContentProps = {
	children: ReactNode;
	className?: string;
};

type KanbanEmptyProps = {
	children: ReactNode;
	className?: string;
};

type KanbanCardProps = {
	children: ReactNode;
	className?: string;
};

// the board scrolls horizontally, so it is focusable - otherwise columns past the
// viewport are unreachable with the keyboard alone
function KanbanBoard({ children, label, className }: KanbanProps) {
	return (
		// biome-ignore lint/a11y/noNoninteractiveTabindex: a horizontally scrolled board has to be reachable with the keyboard
		<section className="kanban-wrapper" tabIndex={0} aria-label={label}>
			<div className={clsx("kanban", className)}>{children}</div>
		</section>
	);
}

function KanbanColumn({ children, label, className }: KanbanColumnProps) {
	return (
		<section className={clsx("kanban-column", className)} aria-label={label}>
			{children}
		</section>
	);
}

function KanbanColumnHeader({ title, icon, count, meta, className }: KanbanColumnHeaderProps) {
	return (
		<header className={clsx("kanban-column-header", className)}>
			<div className="kanban-column-title">
				{icon ? <span className="kanban-column-icon">{icon}</span> : null}

				<span className="kanban-column-name">{title}</span>

				{count === undefined ? null : <span className="kanban-column-count">{count}</span>}
			</div>

			{meta ? <div className="kanban-column-meta">{meta}</div> : null}
		</header>
	);
}

function KanbanColumnContent({ children, className }: KanbanContentProps) {
	return <div className={clsx("kanban-column-content", className)}>{children}</div>;
}

function KanbanColumnFooter({ children, className }: KanbanContentProps) {
	return <div className={clsx("kanban-column-footer", className)}>{children}</div>;
}

function KanbanEmpty({ children, className }: KanbanEmptyProps) {
	return <div className={clsx("kanban-column-empty", className)}>{children}</div>;
}

function KanbanCard({ children, className }: KanbanCardProps) {
	return <article className={clsx("kanban-card", className)}>{children}</article>;
}

export const Kanban = Object.assign(KanbanBoard, {
	Column: KanbanColumn,
	ColumnHeader: KanbanColumnHeader,
	ColumnContent: KanbanColumnContent,
	ColumnFooter: KanbanColumnFooter,
	Empty: KanbanEmpty,
	Card: KanbanCard,
});
