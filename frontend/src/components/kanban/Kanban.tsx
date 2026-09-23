import clsx from "clsx";
import type { ReactNode } from "react";
import {
	DraggableCard,
	DroppableColumn,
	KanbanDnd,
	type KanbanMove,
	useKanbanDrag,
} from "./KanbanDnd";
import "./kanban.css";

export type { KanbanMove } from "./KanbanDnd";

type KanbanProps<T> = {
	children: ReactNode;
	label: string;
	className?: string;
	/** Turns the board into a drag and drop one. Without it columns and cards are plain markup. */
	onMove?: (move: KanbanMove<T>) => void;
	/** Columns a card may not go to are dimmed while it is dragged and refuse the drop. */
	canMove?: (move: KanbanMove<T>) => boolean;
};

type KanbanColumnProps = {
	children: ReactNode;
	label: string;
	/** The value a dropped card moves to; a column without one does not accept drops. */
	id?: string;
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
	/** Set together with `column` to make the card draggable on a board with `onMove`. */
	dragId?: string;
	column?: string;
	/** Handed back in the move, so the feature does not have to find the item again. */
	data?: unknown;
};

// the board scrolls horizontally, so it is focusable - otherwise columns past the
// viewport are unreachable with the keyboard alone
function KanbanBoard<T>({ children, label, className, onMove, canMove }: KanbanProps<T>) {
	const board = (
		// biome-ignore lint/a11y/noNoninteractiveTabindex: a horizontally scrolled board has to be reachable with the keyboard
		<section className="kanban-wrapper" tabIndex={0} aria-label={label}>
			<div className={clsx("kanban", className)}>{children}</div>
		</section>
	);

	if (!onMove) {
		return board;
	}

	// the drag context is untyped; the card that started the drag put a T into `data`
	return (
		<KanbanDnd
			onMove={onMove as (move: KanbanMove) => void}
			canMove={canMove as ((move: KanbanMove) => boolean) | undefined}
		>
			{board}
		</KanbanDnd>
	);
}

function KanbanColumn({ children, label, id, className }: KanbanColumnProps) {
	const drag = useKanbanDrag();

	if (drag && id !== undefined) {
		return (
			<DroppableColumn id={id} label={label} className={className} state={drag}>
				{children}
			</DroppableColumn>
		);
	}

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

function KanbanCard({ children, className, dragId, column, data }: KanbanCardProps) {
	const drag = useKanbanDrag();

	if (drag && dragId !== undefined && column !== undefined) {
		return (
			<DraggableCard id={dragId} column={column} data={data} className={className}>
				{children}
			</DraggableCard>
		);
	}

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
