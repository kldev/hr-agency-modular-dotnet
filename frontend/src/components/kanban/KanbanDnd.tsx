import {
	type CollisionDetection,
	closestCenter,
	DndContext,
	type DragEndEvent,
	DragOverlay,
	type DragStartEvent,
	KeyboardSensor,
	MouseSensor,
	pointerWithin,
	TouchSensor,
	useDraggable,
	useDroppable,
	useSensor,
	useSensors,
} from "@dnd-kit/core";
import clsx from "clsx";
import { createContext, type KeyboardEvent, type ReactNode, useContext, useState } from "react";

/**
 * A card dropped on another column. Nothing has moved yet: the board only reports the intent, and
 * the feature decides what it means - usually a drawer asking for the rest of the command.
 */
export type KanbanMove<T = unknown> = {
	id: string;
	from: string;
	to: string;
	data: T;
};

type DraggedCard = {
	id: string;
	from: string;
	data: unknown;
	preview: ReactNode;
};

type KanbanDragState = {
	active: DraggedCard | null;
	canMove: (move: KanbanMove) => boolean;
};

// null outside a board with `onMove`: columns and cards then render as plain markup
const KanbanDragContext = createContext<KanbanDragState | null>(null);

export function useKanbanDrag() {
	return useContext(KanbanDragContext);
}

interface KanbanDndProps {
	children: ReactNode;
	onMove: (move: KanbanMove) => void;
	canMove?: (move: KanbanMove) => boolean;
}

const allowAll = () => true;

/*
 * The column under the pointer, not the one the dragged card overlaps most: a card grabbed by its
 * left edge hangs over the next column, and the drop went there. The keyboard has no pointer and
 * moves the card itself, so it gets the nearest column instead.
 */
const columnUnderPointer: CollisionDetection = (args) =>
	args.pointerCoordinates ? pointerWithin(args) : closestCenter(args);

export function KanbanDnd({ children, onMove, canMove = allowAll }: KanbanDndProps) {
	const [active, setActive] = useState<DraggedCard | null>(null);

	/*
	 * A card carries a link and an action menu, so a drag starts only after the pointer travelled a
	 * few pixels, and on a touch screen after a press - otherwise a swipe could not scroll the board.
	 */
	const sensors = useSensors(
		useSensor(MouseSensor, { activationConstraint: { distance: 6 } }),
		useSensor(TouchSensor, { activationConstraint: { delay: 250, tolerance: 5 } }),
		useSensor(KeyboardSensor),
	);

	const onDragStart = ({ active: dragged }: DragStartEvent) => {
		const card = dragged.data.current as Omit<DraggedCard, "id"> | undefined;

		setActive(card ? { ...card, id: String(dragged.id) } : null);
	};

	const onDragEnd = ({ over }: DragEndEvent) => {
		const card = active;
		setActive(null);

		if (!card || !over || over.id === card.from) {
			return;
		}

		const move: KanbanMove = { id: card.id, from: card.from, to: String(over.id), data: card.data };

		if (canMove(move)) {
			onMove(move);
		}
	};

	return (
		<KanbanDragContext.Provider value={{ active, canMove }}>
			<DndContext
				sensors={sensors}
				collisionDetection={columnUnderPointer}
				// the board scrolls sideways only near its edges; a card starts close to the left one
				autoScroll={{ threshold: { x: 0.1, y: 0.1 } }}
				onDragStart={onDragStart}
				onDragEnd={onDragEnd}
				onDragCancel={() => setActive(null)}
			>
				{children}

				<DragOverlay>
					{active ? (
						<article className="kanban-card kanban-card-overlay">{active.preview}</article>
					) : null}
				</DragOverlay>
			</DndContext>
		</KanbanDragContext.Provider>
	);
}

type DropState = "origin" | "allowed" | "blocked" | undefined;

function dropState(state: KanbanDragState, column: string): DropState {
	const { active, canMove } = state;

	if (!active) return undefined;
	if (active.from === column) return "origin";

	return canMove({ id: active.id, from: active.from, to: column, data: active.data })
		? "allowed"
		: "blocked";
}

interface DroppableColumnProps {
	id: string;
	label: string;
	className?: string;
	children: ReactNode;
	state: KanbanDragState;
}

export function DroppableColumn({ id, label, className, children, state }: DroppableColumnProps) {
	const drop = dropState(state, id);
	const { setNodeRef, isOver } = useDroppable({ id, disabled: drop === "blocked" });

	return (
		<section
			ref={setNodeRef}
			className={clsx("kanban-column", className)}
			aria-label={label}
			data-drop={drop}
			data-over={isOver && drop === "allowed" ? "" : undefined}
		>
			{children}
		</section>
	);
}

interface DraggableCardProps {
	id: string;
	column: string;
	data: unknown;
	className?: string;
	children: ReactNode;
}

export function DraggableCard({ id, column, data, className, children }: DraggableCardProps) {
	const { setNodeRef, attributes, listeners, isDragging } = useDraggable({
		id,
		data: { from: column, data, preview: children },
	});

	/*
	 * Space and Enter pick the card up - but only on the card itself. Pressed on the link or the
	 * menu button inside it, they bubble up here and must keep doing what those controls do.
	 */
	const onKeyDown = (event: KeyboardEvent<HTMLElement>) => {
		if (event.target === event.currentTarget) {
			listeners?.onKeyDown?.(event);
		}
	};

	return (
		<article
			ref={setNodeRef}
			className={clsx(
				"kanban-card kanban-card-draggable",
				isDragging && "kanban-card-dragging",
				className,
			)}
			{...attributes}
			{...listeners}
			onKeyDown={onKeyDown}
		>
			{children}
		</article>
	);
}
